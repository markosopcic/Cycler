$(document).ready(() => {

    $("#load-more-personal-events-button").click(function(){
        var skip = $(".row").length
        fetch("/api/get-personal-event-feed?skip="+skip+"&take=10").then((response)=>{
            return response.text()
        }).then((result)=> {
            if(result.trim().length == 0){
                alert("No more items to load!")
            }else{
                document.getElementById("events").innerHTML += result;
            }

        })
    })
    
    
    fetch("/api/get-personal-event-feed?skip=0&take=10").then((response)=>{
        return response.text()
    }).then((result)=> {
        document.getElementById("events").innerHTML = result;
    })
    
})