using Xunit;
using System.Xml.Linq;
using RegionHR.IntegrationHub.Adapters.Nordea;

namespace RegionHR.IntegrationHub.Tests;

public class NordeaPaymentTests
{
    private const string PAIN_001_NS = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";

    private static PaymentBatch CreateTestBatch() => new()
    {
        MessageId = "TEST-MSG-001",
        Period = "2026-03",
        OrganizationNumber = "2321000123",
        InitiatorName = "Region Test",
        DebtorName = "Region Test",
        DebtorIBAN = "SE1234567890123456789012",
        ExecutionDate = new DateOnly(2026, 3, 25),
        Payments =
        [
            new SalaryPayment
            {
                PaymentId = "PAY-001",
                RecipientName = "Erik Johansson",
                ClearingNumber = "3300",
                AccountNumber = "1234567890",
                Amount = 28500.00m,
                Period = "2026-03"
            },
            new SalaryPayment
            {
                PaymentId = "PAY-002",
                RecipientName = "Maria Andersson",
                ClearingNumber = "5100",
                AccountNumber = "9876543210",
                Amount = 32000.50m,
                Period = "2026-03"
            }
        ]
    };

    [Fact]
    public void Generate_ProducesValidPain001XmlStructure()
    {
        // Arrange
        var generator = new NordeaPaymentFileGenerator();
        var batch = CreateTestBatch();

        // Act
        var xml = generator.Generate(batch);
        var doc = XDocument.Parse(xml);

        // Assert
        var ns = XNamespace.Get(PAIN_001_NS);
        Assert.NotNull(doc.Root);
        Assert.Equal(ns + "Document", doc.Root.Name);

        // Verify CstmrCdtTrfInitn structure
        var initiation = doc.Root.Element(ns + "CstmrCdtTrfInitn");
        Assert.NotNull(initiation);

        // Verify GrpHdr
        var grpHdr = initiation.Element(ns + "GrpHdr");
        Assert.NotNull(grpHdr);
        Assert.Equal("TEST-MSG-001", grpHdr.Element(ns + "MsgId")?.Value);
        Assert.Equal("2", grpHdr.Element(ns + "NbOfTxs")?.Value);

        // Verify PmtInf
        var pmtInf = initiation.Element(ns + "PmtInf");
        Assert.NotNull(pmtInf);
        Assert.Equal("TRF", pmtInf.Element(ns + "PmtMtd")?.Value);

        // Verify transactions
        var transactions = pmtInf.Elements(ns + "CdtTrfTxInf").ToList();
        Assert.Equal(2, transactions.Count);
    }

    [Fact]
    public void Generate_HasSALACategoryPurpose()
    {
        // Arrange
        var generator = new NordeaPaymentFileGenerator();
        var batch = CreateTestBatch();

        // Act
        var xml = generator.Generate(batch);
        var doc = XDocument.Parse(xml);

        // Assert
        var ns = XNamespace.Get(PAIN_001_NS);
        var pmtInf = doc.Descendants(ns + "PmtInf").First();
        var ctgyPurp = pmtInf.Descendants(ns + "CtgyPurp").First();
        var code = ctgyPurp.Element(ns + "Cd")?.Value;

        Assert.Equal("SALA", code); // SALA = Salary payment
    }

    [Fact]
    public void Generate_ControlSumMatchesPayments()
    {
        // Arrange
        var generator = new NordeaPaymentFileGenerator();
        var batch = CreateTestBatch();
        var expectedSum = batch.Payments.Sum(p => p.Amount);

        // Act
        var xml = generator.Generate(batch);
        var doc = XDocument.Parse(xml);

        // Assert
        var ns = XNamespace.Get(PAIN_001_NS);

        // Verify CtrlSum in GrpHdr
        var grpHdrCtrlSum = doc.Descendants(ns + "GrpHdr").First()
            .Element(ns + "CtrlSum")?.Value;
        Assert.Equal(expectedSum.ToString("F2"), grpHdrCtrlSum);

        // Verify CtrlSum in PmtInf
        var pmtInfCtrlSum = doc.Descendants(ns + "PmtInf").First()
            .Element(ns + "CtrlSum")?.Value;
        Assert.Equal(expectedSum.ToString("F2"), pmtInfCtrlSum);
    }
}
