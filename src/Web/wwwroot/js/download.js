window.downloadFile = function (filename, base64, mimeType) {
    var link = document.createElement('a');
    link.href = 'data:' + (mimeType || 'application/octet-stream') + ';base64,' + base64;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
