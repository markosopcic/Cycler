$(document).ready(()=>{
   $(".accept").click(clickEvent => {
       let onsuccess = function(response){
           $(clickEvent.target.parentNode.parentNode).remove();
           if($(".invitation").length === 0){
               $(".info").show()
           }
       }
       $.ajax({
           url:"/Event/AcceptInvitation",
           data: {invitationId:clickEvent.target.parentNode.parentNode.id,accept:true},
           success:onsuccess,
           error(error){
               alert("Something went wrong! Try again later.");
           }
       })
   });

    $(".decline").click(clickEvent => {
        let onsuccess = function(response){
            $(clickEvent.target.parentNode.parentNode).remove();
            if($(".invitation").length === 0){
                $(".info").show()
            }
        }
        $.ajax({
            url:"/Event/AcceptInvitation",
            data: {invitationId:clickEvent.target.parentNode.parentNode.id,accept:false},
            success:onsuccess,
            error(error){
                alert("Something went wrong! Try again later.");
            }
        })
    });
});