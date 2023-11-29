let logCount = 0;
function WriteToChatLog(message){
    let logElem = document.createElement("p");
    logElem.classList.add("log");
    logElem.id = `log_${++logCount}`;
    logElem.textContent = `${logCount} ${message}`;
    document.querySelector("#chat_log").append(logElem);
}

function AppendPostTurnDataToChatLog(x, y){
    let turnLog = document.querySelector(`#log_${logCount}`);
    turnLog.textContent += `: X-${++x}, Y-${++y}`;
}

document.addEventListener("DOMContentLoaded", function (e) {
    
    let loginFormButton = document.querySelector("#login_button_form");
    let signUpFormButton = document.querySelector("#signup_button_form");
    let previousSelectedMenu;

    if (loginFormButton !== null) {
        loginFormButton.addEventListener("click", function (e) {
            document.querySelector("#login_form").style.display = "flex";
            document.querySelector("#signup_form").style.display = "none";

            signUpFormButton.classList.remove("active_menu");
            this.classList.add("active_menu");
            history.pushState({}, null, `/login`);
        })
    }

    if (signUpFormButton !== null) {
        signUpFormButton.addEventListener("click", function (e) {
            document.querySelector("#login_form").style.display = "none";
            document.querySelector("#signup_form").style.display = "flex";

            loginFormButton.classList.remove("active_menu");
            this.classList.add("active_menu");
            history.pushState({}, null, `/signup`);
        })
    }

    let gamesFormButton = document.querySelector("#games_button_form");
    let accountFormButton = document.querySelector("#account_button_form");
    let createGameButton = document.querySelector("#create_game_form");

    if (gamesFormButton !== null) {
        gamesFormButton.addEventListener("click", function (e) {
            document.querySelector("#games_form").style.display = "flex";
            document.querySelector("#account_statistics").style.display = "none";

            accountFormButton.classList.remove("active_menu");
            createGameButton.classList.remove("active_menu");
            this.classList.add("active_menu");
            previousSelectedMenu = this;
            history.pushState({}, null, `/games`);
        })
    }

    if (accountFormButton !== null) {
        accountFormButton.addEventListener("click", function (e) {
            document.querySelector("#games_form").style.display = "none";
            document.querySelector("#account_statistics").style.display = "flex";

            gamesFormButton.classList.remove("active_menu");
            createGameButton.classList.remove("active_menu");
            this.classList.add("active_menu");
            previousSelectedMenu = this;
            history.pushState({}, null, `/account`);
        })
    }
    if (createGameButton !== null) {
        createGameButton.addEventListener("click", function (e) {

            if (gamesFormButton.classList.contains("active_menu")) {
                previousSelectedMenu = gamesFormButton;
            } else if (accountFormButton.classList.contains("active_menu")) {
                previousSelectedMenu = accountFormButton;
            }

            gamesFormButton.classList.remove("active_menu");
            accountFormButton.classList.remove("active_menu");
            this.classList.add("active_menu");

            let modalElem = document.querySelector("#create_game_modal");
            
            document.querySelector("html").classList.add("overflow_hidden");
            FadeInElem(modalElem);
        })
    }

    let createGameModal = document.querySelector("#create_game_modal");

    if (createGameModal !== null) {
        document.querySelector(".close_game_modal").addEventListener("click", () => createGameModal.click());
        createGameModal.addEventListener("click", function (e) {
            e.stopImmediatePropagation();

            if (e.currentTarget === e.target) {
                createGameButton.classList.remove("active_menu");
                previousSelectedMenu.classList.add("active_menu");

                let modalElem = e.currentTarget;

                document.querySelector("html").classList.remove("overflow_hidden");
                FadeOutElem(modalElem);
            }
        })
    }


    function FadeInElem(elem) {
        elem.style.display = "flex";

        setTimeout(function () {
            elem.style.opacity = 1;
        }, 1);
    }

    function FadeOutElem(elem) {
        elem.style.opacity = 0;

        setTimeout(function () {
            elem.style.display = "none";
        }, 300);
    }

})