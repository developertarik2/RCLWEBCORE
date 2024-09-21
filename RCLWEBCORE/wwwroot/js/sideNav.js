let arrow = document.querySelectorAll(".arrow");
for (var i = 0; i < arrow.length; i++) {
    arrow[i].addEventListener("click", (e) => {
        let arrowParent = e.target.parentElement.parentElement;//selecting main parent of arrow
        arrowParent.classList.toggle("showMenu");
    });
}

let sidebar = document.querySelector(".sidebar");
let sidebarBtn = document.querySelector(".fa-bars");
console.log(sidebarBtn);
sidebarBtn.addEventListener("click", () => {
    sidebar.classList.toggle("close");
    menuBtnChange()
});
const closeBtn = document.querySelector("#cc");
const home = document.querySelector("#hh");
function menuBtnChange() {
    if (sidebar.classList.contains("close")) {
        closeBtn.classList.replace("fa-xmark", "fa-bars")
    } else {
       
        closeBtn.classList.replace("fa-bars", "fa-xmark")
    }
}


//$('#cc').click.fu addEventListener("click", ()=> {
//    sidebar.classList.toggle("close");
//})
if (($(window).width() <= 800)) {
    
    home.classList.replace("body-trimmed", "body-md-screen")
  
}
if (($(window).width() > 800)) {
    home.classList.replace("body-md-screen", "body-trimmed")
    
   
}
$(window).resize(function () {
    if (($(window).width() <= 800)) {
        home.classList.replace("body-trimmed", "body-md-screen")
       
    }
    if (($(window).width() > 800)) {
        home.classList.replace("body-md-screen", "body-trimmed")
        
        
    }
});



