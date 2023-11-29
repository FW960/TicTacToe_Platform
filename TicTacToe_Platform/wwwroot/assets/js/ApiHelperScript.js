function ValidateLoginData(loginElem, passwordElem) {

    let isIncorrect = false;

    if (loginElem.value === "") {
        loginElem.classList.add("data_validation_error");
        isIncorrect = true;
    } else {
        loginElem.classList.remove("data_validation_error")
    }

    if (passwordElem.value === "") {
        passwordElem.classList.add("data_validation_error");
        isIncorrect = true;
    } else {
        passwordElem.classList.remove("data_validation_error")
    }

    return isIncorrect;
}

function ValidateSignUpData(signUpElem, passwordElem, confirmPasswordElem) {

    let isIncorrect = false;

    isIncorrect = ValidateLoginData(signUpElem, passwordElem);

    if (confirmPasswordElem.value === "") {
        confirmPasswordElem.classList.add("data_validation_error");
        isIncorrect = true;
    } else {
        confirmPasswordElem.classList.remove("data_validation_error")
    }
    
    if (confirmPasswordElem.value !== passwordElem.value) {
        confirmPasswordElem.classList.add("data_validation_error");
        passwordElem.classList.add("data_validation_error");
        isIncorrect = true;
    } else if(confirmPasswordElem.value !== "" && passwordElem.value !== "") {
        confirmPasswordElem.classList.remove("data_validation_error");
        passwordElem.classList.remove("data_validation_error");
    }

    return isIncorrect;

}
function BaseValidateField(field){
    if (field.value === "") {
        field.classList.add("data_validation_error");
        return true;
    } else {
        field.classList.remove("data_validation_error")
    }
}
function ConstructFetchBody(method, headers, body) {

    headers["X-FETCH-INDICATOR"] = "true";

    if (method.toLowerCase() === "get") {

        return {
            headers: headers,
            method: method
        }
    }

    if (method.toLowerCase() === "post") {


        if (typeof body === "object") {
            body = JSON.stringify(body);
            headers["Content-Type"] = "application/json";
        }

        return {
            body: body,
            headers: headers,
            method: method
        }
    }

}

