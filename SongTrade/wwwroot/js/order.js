var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Order/GetAll"
        },
        "columns": [
            { "data": "id", "width": "30%" },
            { "data": "createdAt", "width": "30%" },
            { "data": "orderTotal", "width": "40%" },
        ]
    })
}