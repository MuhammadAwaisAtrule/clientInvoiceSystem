window.downloadFileFromStream = async (fileName, streamRef, contentType) => {
    const data = await streamRef.arrayBuffer();
    const blob = new Blob([data], { type: contentType });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
};
