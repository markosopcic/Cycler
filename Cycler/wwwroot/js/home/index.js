$(document).ready(() => {
    
    
    $("#load-more-feed-button").click(function(){
        var skip = $(".feed-item").length
        fetch("/api/get-event-feed?skip="+skip+"&take=10").then((response)=>{
            return response.text()
        }).then((result)=> {
            if(result.trim().length == 0){
                alert("No more items to load!")
            }else{
                document.getElementById("feed").innerHTML += result;
            }

        })
    })
    
    fetch("/api/get-event-feed?skip=0&take=10").then((response)=>{
        return response.text()
    }).then((result)=> {
        document.getElementById("feed").innerHTML = result;
    })
})