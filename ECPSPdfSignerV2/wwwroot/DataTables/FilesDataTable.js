window.DataTables1 = {
    dataTable: null,
    buildDataTable: function (dotNetHelper) {
        this.destroyDataTable();
        this.dataTable = $("#files_table").DataTable({
            responsive: true,
            select: {
                style: 'single'
            },
            columns: [
                { visible: true }, 
                { visible: true },
                { visible: true },
                { visible: true },
                { visible: true },
                { visible: false }
            ],
            createdRow: function (row, data, dataIndex) {
                $(row).find('td').addClass('custom-td');
            }
        });

        // Listen for click events on the table cells
        $('#files_table tbody').on('click', 'td:first-child', function () {
            var table = $('#files_table').DataTable();
            var cell = table.cell(this); // Get the cell that was clicked
            var row = cell.index().row; // Get the row index of the clicked cell
            var selectedRowData = table.row(row).data(); // Get the data of the selected row

            // Call the SelectRow() method in your Blazor component
            dotNetHelper.invokeMethodAsync('SelectRow', selectedRowData[0], selectedRowData[5]);
        });
    },
    destroyDataTable: function () {
        if (this.dataTable && $.fn.DataTable.isDataTable('#files_table')) {
            this.dataTable.destroy();
            this.dataTable = null; // Reset the dataTable variable
        }
    }
}

// Call the buildDataTable method to initialize the DataTable
window.DataTables1.buildDataTable();