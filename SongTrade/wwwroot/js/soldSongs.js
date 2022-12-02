var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Song/GetSoldSongs"
        },
        "columns": [
            { "data": "title", "width": "40%" },
            { "data": "soldCount", "width": "30%" },
            { "data": "totalPrice", "width": "30%" }
        ]
    })
}