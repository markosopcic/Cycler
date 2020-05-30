$(document).ready(function(){
   $("#invited-users").select2({
      width:'resolve',
      placeholder:"Search users",
      paddingRight:'resolve',
      height:"resolve",
      minimumInputLength: 3,
      ajax: {
         url: '/api/search-friends',
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
});