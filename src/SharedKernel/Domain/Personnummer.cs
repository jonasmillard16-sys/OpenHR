namespace RegionHR.SharedKernel.Domain;

/// <summary>
/// Svenskt personnummer (YYYYMMDD-NNNN) med Luhn-validering.
/// Stöder samordningsnummer (dag + 60).
/// Lagras krypterat i databas.
/// </summary>
public sealed record Personnummer
{
    // Store as YYYYMMDDNNNN (12 digits)
    private readonly string _value;

    public Personnummer(string input)
    {
        var normalized = Normalize(input);
        if (!IsValid(normalized))
            throw new ArgumentException($"Ogiltigt personnummer: {input}");
        _value = normalized;
    }

    public int Year => int.Parse(_value[..4]);
    public int Month => int.Parse(_value[4..6]);
    public int Day => int.Parse(_value[6..8]);
    public int BirthDay => Day > 60 ? Day - 60 : Day; // Samordningsnummer
    public bool IsSamordningsnummer => Day > 60;
    public string LastFour => _value[8..];

    /// <summary>Juridiskt kön baserat på näst sista siffran (udda = man, jämn = kvinna)</summary>
    public string LegalGender => int.Parse(_value[10..11]) % 2 == 0 ? "Kvinna" : "Man";

    public DateOnly BirthDate => new(Year, Month, BirthDay);

    /// <summary>Format: YYYYMMDD-NNNN</summary>
    public override string ToString() => $"{_value[..8]}-{_value[8..]}";

    /// <summary>Maskerat format: YYYYMMDD-****</summary>
    public string ToMaskedString() => $"{_value[..8]}-****";

    private static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Personnummer får inte vara tomt.");

        // Remove whitespace, hyphens, plus signs
        var cleaned = input.Replace(" ", "").Replace("-", "").Replace("+", "");

        if (cleaned.Length == 10)
        {
            // Determine century: if the original had '+' separator, it means 100+ years old
            var hasPlus = input.Contains('+');
            var twoDigitYear = int.Parse(cleaned[..2]);
            var currentYear = DateTime.Now.Year % 100;
            var currentCentury = DateTime.Now.Year / 100;

            int century;
            if (hasPlus)
            {
                century = twoDigitYear <= currentYear ? (currentCentury - 1) : (currentCentury - 2);
            }
            else
            {
                century = twoDigitYear <= currentYear ? currentCentury : (currentCentury - 1);
            }

            cleaned = $"{century}{cleaned}";
        }

        if (cleaned.Length != 12 || !cleaned.All(char.IsDigit))
            throw new ArgumentException($"Ogiltigt personnummerformat: {input}");

        return cleaned;
    }

    private static bool IsValid(string twelveDigits)
    {
        // Validate date part
        var year = int.Parse(twelveDigits[..4]);
        var month = int.Parse(twelveDigits[4..6]);
        var day = int.Parse(twelveDigits[6..8]);

        // Samordningsnummer: day + 60
        var actualDay = day > 60 ? day - 60 : day;

        if (month < 1 || month > 12 || actualDay < 1 || actualDay > 31)
            return false;

        try
        {
            _ = new DateOnly(year, month, actualDay);
        }
        catch
        {
            return false;
        }

        // Luhn check on last 10 digits (YYMMDDNNNN)
        var luhnInput = twelveDigits[2..];
        return LuhnCheck(luhnInput);
    }

    private static bool LuhnCheck(string digits)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            var digit = digits[i] - '0';
            // Alternate multiplier: 2,1,2,1,... starting from first digit
            var multiplied = digit * (i % 2 == 0 ? 2 : 1);
            sum += multiplied > 9 ? multiplied - 9 : multiplied;
        }
        return sum % 10 == 0;
    }

    // Implicit conversion support
    public static implicit operator string(Personnummer p) => p._value;
}
