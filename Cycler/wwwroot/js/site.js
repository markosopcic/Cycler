// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(() => {
   $("#nav-search").select2({
       width:'resolve',
       placeholder:"Search users",
       paddingRight:'resolve',
       height:"resolve",
       minimumInputLength: 3,
       ajax: {
           url: '/api/search-users',
           data: function (params) {
               if (params.term === null || params.term === undefined || params.term === "")
                   params.term = "";
               var query = {
                   term: params.term
               };

               // Query parameters will be ?searchTerm=[term]
               return query;
           },
           delay: 250, // wait 250 milliseconds before triggering the request
           processResults: function (data, params) {
               // Transforms the top-level key of the response object from 'items' to 'results'
               return {
                   results: $.map(data, function (item) {
                       return {
                           id: item.id,
                           text: item.fullName,
                           "data-modeltype": item.resultType
                       };
                   })
               };
           }
       },
       
   });
   


    $('#nav-search').on('select2:select', function (e) {
        if (e.params.data === null || e.params.data === undefined) {
            alert("There has been a server error. Please reload the page and try again");
            return;
        }

        var data = {
            id: e.params.data.id,
            modelType: e.params.data["data-modeltype"]
        };

        if (data === null || data === undefined || data.id === null || data.id === undefined) {
            alert("Search result selected could not be opened. Please reload the page and try again");
            return;
        }

        if (data.modelType === "User")
            window.location = "/User/Profile/" + data.id;
        else
            alert("Search result selected could not be opened. Please reload the page and try again");
    });
});