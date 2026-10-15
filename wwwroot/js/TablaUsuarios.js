// < type="text/javascript">
$(document).ready(function () {
    $('#TablaUsuariosContainer').jtable({
        title: 'Lista de Usuarios',
        paging: true, // Habilitar paginación
        pageSize: 10, // Registros por página
        sorting: true, // Habilitar ordenamiento
        defaultSorting: 'Id ASC',
        actions: {
            listAction: '/Usuarios/ListarUsuarios',
            createAction: '/Usuarios/CrearUsuario',
            updateAction: '/Usuarios/ActualizarUsuario',
            deleteAction: '/Usuarios/EliminarUsuario'
        },
        fields: {
            Id: {
                key: true,
                create: false,
                edit: false,
                list: false
            },
            Nombre: {
                title: 'Nombre',
                width: '30%'
            },
            Email: {
                title: 'Correo Electrónico',
                width: '30%'
            },
            FechaRegistro: {
                title: 'Fecha de Registro',
                type: 'date',
                displayFormat: 'yy-mm-dd',
                create: false,
                edit: false
            }
        }
    });

    // Cargar los datos automáticamente al abrir la página
    $('#TablaUsuariosContainer').jtable('load');
});
