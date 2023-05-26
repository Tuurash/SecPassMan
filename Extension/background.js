import * as _ from "./browser-polyfill.min.js";

const socket = new WebSocket('ws://localhost:8080');

// const connect = () => {
//     const socket = new WebSocket('ws://localhost:8080');
//     socket.onopen=() => {
//         console.log('WebSocket connection opened');
//         // Send a ping message every 30 seconds
//         setInterval(() => {
//             socket.send('ping');
//         }, 7000);
//     };
    
//     socket.onmessage=(event) => {
//         const message = JSON.parse(event.data);
//         console.log('Received message:', message);
    
//         //if message is ping, send pong
//         if (message.type === 'pong') {
//             //Do nothing for now
//         }
//         if (message.type === 'siteCreds') {
//             // TODO: Handle login message
//             //If its a list, populate the list
//             if (message.list) {
//                 siteCreds = message.list;
//             }
//         }
//     };
    
//     socket.onclose=(event) => {
//         console.log('WebSocket connection closed');
//         //retry connection
//         setTimeout(() => {
//             connect;
//         }, 5000);
//     };

//     socket.onerror=(error) => {

//         //retry connection
//         setTimeout(() => {
//             connect;
//         }, 5000);
//     };
// }

socket.addEventListener('open', (event) => {
        console.log('WebSocket connection opened');
        // Send a ping message every 30 seconds
        setInterval(() => {
            socket.send('ping');
        }, 7000);
    });
    
    socket.addEventListener('message', (event) => {
        const message = JSON.parse(event.data);
        console.log('Received message:', message);
    
        //if message is ping, send pong
        if (message.type === 'pong') {
            //Do nothing for now
        }
        if (Array.isArray(message.SiteCredentials)) {
            // TODO: Handle login message
            //If its a list, populate the list
            siteCreds=message.SiteCredentials;
        }
    });
    
    socket.addEventListener('close', (event) => {
        console.log('WebSocket connection closed');
        //retry connection
        setTimeout(() => {
            const socket=new WebSocket('ws://localhost:8080');
        }, 5000);
    });

    socket.addEventListener('error', (event) => {

        //retry connection
        setTimeout(() => {
            const socket=new WebSocket('ws://localhost:8080');
        }, 5000);
    });

class sitecredential {
    constructor(username, password, url) {
        this.username = username;
        this.password = password;
        this.url = url;
    }
}

let siteCreds=[];

//connect();


browser.tabs.onUpdated.addListener(async (tabId, changeInfo, tab) => {
    if (changeInfo.url) {
        await handleUrlChange(tabId, changeInfo.url);
    }
});

let SiteCredObj;

async function handleUrlChange(tabid, url) {
    if (url.startsWith("http")) {


        SiteCredObj = siteCreds.find(s => url.includes(s.SiteUrl));
        if (SiteCredObj) {
            browser.scripting.executeScript({
                target: { tabId: tabid },
                function: autofill,
                args: [SiteCredObj],
            });
        } else {
            // Send uri to server
            //check if sccket is open
            if (socket.readyState === WebSocket.OPEN) {
                socket.send(JSON.stringify({ type: 'url', url }));
            }
        }


    }
}

const autofill = async (SiteCredObj) => {
    //Find username and password fields and AuoFill
    const usernameInput = document.querySelector("input[type='text']");
    const passwordInput = document.querySelector("input[type='password']");
    usernameInput.value = SiteCredObj.SiteUsername;
    passwordInput.value = SiteCredObj.SitePassword;
}





const newPageLoad = async () => {
    let inputs = document.getElementsByTagName("input");
    const inputLength = inputs.length;
    for (let i = 0; i < inputLength; i++) {
        const input = inputs.item(i);
        if (input.type !== "password") continue;

        const { passwords } = await chrome.storage.sync.get("passwords");
        const pagePassword = passwords.find(password => password.url === location.href);

        if (pagePassword !== undefined) {
            input.value = pagePassword.password;
        } else {
            const popupDiv = document.createElement("div");
            popupDiv.style.position = "absolute";
            const inputRect = input.getBoundingClientRect();
            popupDiv.style.left = inputRect.left + "px";
            popupDiv.style.top = inputRect.top - (inputRect.height + 120) + "px";
            popupDiv.style.backgroundColor = "white";
            popupDiv.style.width = "250px";
            popupDiv.style.height = "120px";
            popupDiv.style.padding = "10px";
            popupDiv.style.borderRadius = "5px";
            popupDiv.style.border = "solid 1px black";

            const title = document.createElement("p");
            title.innerText = "Enter password for this page";

            const passwordInput = document.createElement("input");
            passwordInput.type = "password";

            const addPasswordButton = document.createElement("button");
            addPasswordButton.innerText = "Add password";

            const goAwayButton = document.createElement("button");
            goAwayButton.innerText = "fuck off";
            goAwayButton.addEventListener("click", () => {
                popupDiv.remove();
            });

            popupDiv.appendChild(title);
            popupDiv.appendChild(passwordInput);
            popupDiv.appendChild(addPasswordButton);
            popupDiv.appendChild(goAwayButton);

            document.body.appendChild(popupDiv);

            addPasswordButton.addEventListener("click", () => {
                if (passwordInput.value.length < 8) {
                    alert("Password must be at least 8 characters.");
                    return;
                }

                passwords.push({ password: passwordInput.value, url: location.href });
                chrome.storage.sync.set({ passwords });

                popupDiv.remove();
                input.value = passwordInput.value;
            })
        }
    }
}