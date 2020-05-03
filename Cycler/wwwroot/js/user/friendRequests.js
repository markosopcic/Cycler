$(document).ready(()=> {
    $(".accept,.decline").click(clickEvent => {
        let onsuccess = function(response){
            $(clickEvent.target.parentNode).remove();
            if($(".request").length === 0){
                $(".info").show()
            }
        }
        $.ajax({
            url:"/User/AcceptFriendRequest",
            data: {fromUser:clickEvent.target.id,accept:$(clickEvent.target).hasClass("accept")},
            success:onsuccess,
            error(error){
                alert("Something went wrong! Try again later.");
            }
        })
    });
});