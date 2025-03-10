$(document).ready(function () {
    const rowsPerPage = 5; // Set rows per page
    let currentPage = 1;

    function updateTableRows() {
        return $("#clientTable tbody tr").filter(":visible").toArray();
    }

    function displayPage(page) {
        let tableRows = updateTableRows();
        const start = (page - 1) * rowsPerPage;
        const end = start + rowsPerPage;
        tableRows.forEach((row, index) => {
            $(row).toggle(index >= start && index < end);
        });
    }

    function setupPagination() {
        let tableRows = updateTableRows();
        const pageCount = Math.ceil(tableRows.length / rowsPerPage);
        const pagination = $("#pagination");
        pagination.empty();

        if (pageCount <= 1) return;

        function createPageItem(text, page, disabled, active) {
            return `<li class="page-item ${disabled ? "disabled" : ""} ${active ? "active" : ""}">
                        <a class="page-link" href="#">${text}</a>
                    </li>`;
        }

        pagination.append(createPageItem("❮", currentPage - 1, currentPage === 1));
        for (let i = 1; i <= pageCount; i++) {
            pagination.append(createPageItem(i, i, false, i === currentPage));
        }
        pagination.append(createPageItem("❯", currentPage + 1, currentPage === pageCount));

        $(".page-item a").on("click", function (e) {
            e.preventDefault();
            const pageNum = $(this).text();
            if (pageNum === "❮" && currentPage > 1) currentPage--;
            else if (pageNum === "❯" && currentPage < pageCount) currentPage++;
            else if (!isNaN(pageNum)) currentPage = parseInt(pageNum);
            displayPage(currentPage);
            setupPagination();
        });
    }

    function updatePagination() {
        currentPage = 1;
        displayPage(currentPage);
        setupPagination();
    }

    $("#searchInput").on("keyup", function () {
        var value = $(this).val().toLowerCase();
        var hasMatch = false;

        $("#clientTable tbody tr").each(function () {
            var rowText = $(this).text().toLowerCase();
            var isMatch = rowText.indexOf(value) > -1;
            $(this).toggle(isMatch);
            if (isMatch) {
                hasMatch = true;
            }
        });

        $("#noResults").toggleClass("d-none", hasMatch);
        updatePagination();
    });

    updatePagination();
});
