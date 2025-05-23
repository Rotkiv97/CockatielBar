import { RtlScrollAxisType } from '@angular/cdk/platform';
import { gsap } from 'gsap';

function initLoginAnimations() {
    interface Coordinates {
        x: number;
        y: number;
    }

    var email = document.querySelector('#login__email') as HTMLInputElement,
        password = document.querySelector('#login__password') as HTMLInputElement,
        mySVG = document.querySelector('.cartoon-container') as HTMLElement,
        armL = document.querySelector('.armL') as HTMLElement,
        armR = document.querySelector('.armR') as HTMLElement,
        eyeL = document.querySelector('.eyeL') as HTMLElement,
        eyeR = document.querySelector('.eyeR') as HTMLElement,
        nose = document.querySelector('.nose') as HTMLElement,
        mouth = document.querySelector('.mouth') as HTMLElement,
        mouthBG = document.querySelector('.mouthBG') as HTMLElement,
        mouthSmallBG = document.querySelector('.mouthSmallBG') as HTMLElement,
        mouthMediumBG = document.querySelector('.mouthMediumBG') as HTMLElement,
        mouthLargeBG = document.querySelector('.mouthLargeBG') as HTMLElement,
        mouthMaskPath = document.querySelector('#mouthMaskPath') as SVGPathElement,
        mouthOutline = document.querySelector('.mouthOutline') as HTMLElement,
        tooth = document.querySelector('.tooth') as HTMLElement,
        tongue = document.querySelector('.tongue') as HTMLElement,
        chin = document.querySelector('.chin') as HTMLElement,
        face = document.querySelector('.face') as HTMLElement,
        eyebrow = document.querySelector('.eyebrow') as HTMLElement,
        outerEarL = document.querySelector('.earL .outerEar') as HTMLElement,
        outerEarR = document.querySelector('.earR .outerEar') as HTMLElement,
        earHairL = document.querySelector('.earL .earHair') as HTMLElement,
        earHairR = document.querySelector('.earR .earHair') as HTMLElement,
        hair = document.querySelector('.hair') as HTMLElement,
        isPasswordVisible = false;
        
    var caretPos, curEmailIndex, screenCenter, svgCoords, eyeMaxHorizD = 20,
        eyeMaxVertD = 10,
        noseMaxHorizD = 23,
        noseMaxVertD = 10,
        dFromC, eyeDistH, eyeLDistV, eyeRDistV, eyeDistR, mouthStatus = "small";

    function getCoord(e?: any) {
        var carPos = email?.selectionEnd,
            div = document.createElement('div'),
            span = document.createElement('span'),
            copyStyle = getComputedStyle(email as Element),
            emailCoords: Coordinates = { x: 0, y: 0 },
            caretCoords: Coordinates = { x: 0, y: 0 },
            centerCoords: Coordinates = { x: 0, y: 0 };
        
        [].forEach.call(copyStyle, function (prop) {
            div.style[prop as any] = copyStyle[prop];
        });
        
        div.style.position = 'absolute';
        document.body.appendChild(div);
        div.textContent = email?.value.substr(0, carPos as number);
        span.textContent = email?.value.substr(carPos as number) || '.';
        div.appendChild(span);

        emailCoords = getPosition(email as HTMLElement);
        caretCoords = getPosition(span);
        centerCoords = getPosition(mySVG as HTMLElement);
        svgCoords = getPosition(mySVG as HTMLElement);
        screenCenter = centerCoords.x + ((mySVG as HTMLElement).offsetWidth / 2);
        caretPos = caretCoords.x + emailCoords.x;

        dFromC = screenCenter - caretPos;
        var pFromC = Math.round((caretPos / screenCenter) * 100) / 100;
        if (pFromC < 1) {
            // Do nothing
        } else if (pFromC > 1) {
            pFromC -= 2;
            pFromC = Math.abs(pFromC);
        }

        eyeDistH = -dFromC * .05;
        if (eyeDistH > eyeMaxHorizD) {
            eyeDistH = eyeMaxHorizD;
        } else if (eyeDistH < -eyeMaxHorizD) {
            eyeDistH = -eyeMaxHorizD;
        }

        var eyeLCoords = {
            x: svgCoords.x + 84,
            y: svgCoords.y + 76
        };
        var eyeRCoords = {
            x: svgCoords.x + 113,
            y: svgCoords.y + 76
        };
        var noseCoords = {
            x: svgCoords.x + 97,
            y: svgCoords.y + 81
        };
        var mouthCoords = {
            x: svgCoords.x + 100,
            y: svgCoords.y + 100
        };
        var eyeLAngle = getAngle(eyeLCoords.x, eyeLCoords.y, emailCoords.x + caretCoords.x, emailCoords.y + 25);
        var eyeLX = Math.cos(eyeLAngle) * eyeMaxHorizD;
        var eyeLY = Math.sin(eyeLAngle) * eyeMaxVertD;
        var eyeRAngle = getAngle(eyeRCoords.x, eyeRCoords.y, emailCoords.x + caretCoords.x, emailCoords.y + 25);
        var eyeRX = Math.cos(eyeRAngle) * eyeMaxHorizD;
        var eyeRY = Math.sin(eyeRAngle) * eyeMaxVertD;
        var noseAngle = getAngle(noseCoords.x, noseCoords.y, emailCoords.x + caretCoords.x, emailCoords.y + 25);
        var noseX = Math.cos(noseAngle) * noseMaxHorizD;
        var noseY = Math.sin(noseAngle) * noseMaxVertD;
        var mouthAngle = getAngle(mouthCoords.x, mouthCoords.y, emailCoords.x + caretCoords.x, emailCoords.y + 25);
        var mouthX = Math.cos(mouthAngle) * noseMaxHorizD;
        var mouthY = Math.sin(mouthAngle) * noseMaxVertD;
        var mouthR = Math.cos(mouthAngle) * 6;
        var chinX = mouthX * .8;
        var chinY = mouthY * .5;
        var chinS = 1 - ((dFromC * .15) / 100);
        if (chinS > 1) {
            chinS = 1 - (chinS - 1);
        }
        var faceX = mouthX * .3;
        var faceY = mouthY * .4;
        var faceSkew = Math.cos(mouthAngle) * 5;
        var eyebrowSkew = Math.cos(mouthAngle) * 25;
        var outerEarX = Math.cos(mouthAngle) * 4;
        var outerEarY = Math.cos(mouthAngle) * 5;
        var hairX = Math.cos(mouthAngle) * 6;
        var hairS = 1.2;

        gsap.to(eyeL, {
            duration: 1,
            x: -eyeLX,
            y: -eyeLY,
            ease: "expo.out"
        });
        gsap.to(eyeR, {
            duration: 1,
            x: -eyeRX,
            y: -eyeRY,
            ease: "expo.out"
        });
        gsap.to(nose, {
            duration: 1,
            x: -noseX,
            y: -noseY,
            rotation: mouthR,
            transformOrigin: "center center",
            ease: "expo.out"
        });
        gsap.to(mouth, {
            duration: 1,
            x: -mouthX,
            y: -mouthY,
            rotation: mouthR,
            transformOrigin: "center center",
            ease: "expo.out"
        });
        gsap.to(chin, {
            duration: 1,
            x: -chinX,
            y: -chinY,
            scaleY: chinS,
            ease: "expo.out"
        });
        gsap.to(face, {
            duration: 1,
            x: -faceX,
            y: -faceY,
            skewX: -faceSkew,
            transformOrigin: "center top",
            ease: "expo.out"
        });
        gsap.to(eyebrow, {
            duration: 1,
            x: -faceX,
            y: -faceY,
            skewX: -eyebrowSkew,
            transformOrigin: "center top",
            ease: "expo.out"
        });
        gsap.to(outerEarL, {
            duration: 1,
            x: outerEarX,
            y: -outerEarY,
            ease: "expo.out"
        });
        gsap.to(outerEarR, {
            duration: 1,
            x: outerEarX,
            y: outerEarY,
            ease: "expo.out"
        });
        gsap.to(earHairL, {
            duration: 1,
            x: -outerEarX,
            y: -outerEarY,
            ease: "expo.out"
        });
        gsap.to(earHairR, {
            duration: 1,
            x: -outerEarX,
            y: outerEarY,
            ease: "expo.out"
        });
        gsap.to(hair, {
            duration: 1,
            x: hairX,
            scaleY: hairS,
            transformOrigin: "center bottom",
            ease: "expo.out"
        });

        document.body.removeChild(div);
    };
    setupPasswordToggle();

    function onEmailInput(e: Event) {
        getCoord(e);
        var value = (e.target as HTMLInputElement).value;
        curEmailIndex = value.length;
        
        // very crude email validation for now to trigger effects
        if (curEmailIndex > 0) {
            if (mouthStatus == "small") {
                mouthStatus = "medium";
                gsap.to([mouthBG, mouthOutline, mouthMaskPath], {
                    duration: 1,
                    scale: 1.1,  // Anima la scala
                    opacity: 0.8, // O l'opacità
                    ease: "expo.out"
                });
                gsap.to(tooth, {
                    duration: 1,
                    x: 0,
                    y: 0,
                    ease: "expo.out"
                });
                gsap.to(tongue, {
                    duration: 1,
                    x: 0,
                    y: 1,
                    ease: "expo.out"
                });
                gsap.to([eyeL, eyeR], {
                    duration: 1,
                    scaleX: 0.85,
                    scaleY: 0.85,
                    ease: "expo.out"
                });
            }
            if (value.includes("@")) {
                mouthStatus = "large";
                gsap.to([mouthBG, mouthOutline, mouthMaskPath], {
                    duration: 1,
                    ease: "expo.out"
                });
                gsap.to(tooth, {
                    duration: 1,
                    x: 3,
                    y: -2,
                    ease: "expo.out"
                });
                gsap.to(tongue, {
                    duration: 1,
                    y: 2,
                    ease: "expo.out"
                });
                gsap.to([eyeL, eyeR], {
                    duration: 1,
                    scaleX: 0.65,
                    scaleY: 0.65,
                    ease: "expo.out",
                    transformOrigin: "center center"
                });
            } else {
                mouthStatus = "medium";
                gsap.to([mouthBG, mouthOutline, mouthMaskPath], {
                    duration: 1,
                    ease: "expo.out"
                });
                gsap.to(tooth, {
                    duration: 1,
                    x: 0,
                    y: 0,
                    ease: "expo.out"
                });
                gsap.to(tongue, {
                    duration: 1,
                    x: 0,
                    y: 1,
                    ease: "expo.out"
                });
                gsap.to([eyeL, eyeR], {
                    duration: 1,
                    scaleX: 0.85,
                    scaleY: 0.85,
                    ease: "expo.out"
                });
            }
        } else {
            mouthStatus = "small";
            gsap.to([mouthBG, mouthOutline, mouthMaskPath], {
                duration: 1,
                shapeIndex: 9,
                ease: "expo.out"
            });
            gsap.to(tooth, {
                duration: 1,
                x: 0,
                y: 0,
                ease: "expo.out"
            });
            gsap.to(tongue, {
                duration: 1,
                y: 0,
                ease: "expo.out"
            });
            gsap.to([eyeL, eyeR], {
                duration: 1,
                scaleX: 1,
                scaleY: 1,
                ease: "expo.out"
            });
        }
    }

    function fixxateCoord() {
        svgCoords = getPosition(mySVG as HTMLElement);
        var eyeLCoords = {
            x: svgCoords.x + 84,
            y: svgCoords.y + 76
        };
        var eyeRCoords = {
            x: svgCoords.x + 113,
            y: svgCoords.y + 76
        };
        var noseCoords = {
            x: svgCoords.x + 97,
            y: svgCoords.y + 81
        };
        var mouthCoords = {
            x: svgCoords.x + 100,
            y: svgCoords.y + 100
        };
        var fixedX = svgCoords.x + 100; // Centro approssimativo
        var fixedY = svgCoords.y + 120; // Punto leggermente sotto il centro
    
        // Calcola gli angoli rispetto alle parti del viso
        var eyeLAngle = getAngle(eyeLCoords.x, eyeLCoords.y, fixedX, fixedY);
        var eyeLX = Math.cos(eyeLAngle) * eyeMaxHorizD;
        var eyeLY = Math.sin(eyeLAngle) * eyeMaxVertD;
        
        var eyeRAngle = getAngle(eyeRCoords.x, eyeRCoords.y, fixedX, fixedY);
        var eyeRX = Math.cos(eyeRAngle) * eyeMaxHorizD;
        var eyeRY = Math.sin(eyeRAngle) * eyeMaxVertD;
        
        var noseAngle = getAngle(noseCoords.x, noseCoords.y, fixedX, fixedY);
        var noseX = Math.cos(noseAngle) * noseMaxHorizD;
        var noseY = Math.sin(noseAngle) * noseMaxVertD;
        
        var mouthAngle = getAngle(mouthCoords.x, mouthCoords.y, fixedX, fixedY);
        var mouthX = Math.cos(mouthAngle) * noseMaxHorizD;
        var mouthY = Math.sin(mouthAngle) * noseMaxVertD;
        var mouthR = Math.cos(mouthAngle) * 6;
        
        // Resto delle trasformazioni (chin, face, ecc...)
        var chinX = mouthX * .8;
        var chinY = mouthY * .5;
        var chinS = 1;
        var faceX = mouthX * .3;
        var faceY = mouthY * .4;
        var faceSkew = Math.cos(mouthAngle) * 5;
        var eyebrowSkew = Math.cos(mouthAngle) * 25;
        var outerEarX = Math.cos(mouthAngle) * 4;
        var outerEarY = Math.cos(mouthAngle) * 5;
        var hairX = Math.cos(mouthAngle) * 6;
        var hairS = 1.2;
    
        // Applica le animazioni con GSAP (come nel codice originale)
        gsap.to(eyeL, {
            duration: 1,
            x: -eyeLX,
            y: -eyeLY,
            ease: "expo.out"
        });
            gsap.to(eyeR, {
                duration: 1,
                x: -eyeRX,
                y: -eyeRY,
                ease: "expo.out"
            });
            gsap.to(nose, {
                duration: 1,
                x: -noseX,
                y: -noseY,
                rotation: mouthR,
                transformOrigin: "center center",
                ease: "expo.out"
            });
            gsap.to(mouth, {
                duration: 1,
                x: -mouthX,
                y: -mouthY,
                rotation: mouthR,
                transformOrigin: "center center",
                ease: "expo.out"
            });
            gsap.to(chin, {
                duration: 1,
                x: -chinX,
                y: -chinY,
                scaleY: chinS,
                ease: "expo.out"
            });
            gsap.to(face, {
                duration: 1,
                x: -faceX,
                y: -faceY,
                skewX: -faceSkew,
                transformOrigin: "center top",
                ease: "expo.out"
            });
            gsap.to(eyebrow, {
                duration: 1,
                x: -faceX,
                y: -faceY,
                skewX: -eyebrowSkew,
                transformOrigin: "center top",
                ease: "expo.out"
            });
            gsap.to(outerEarL, {
                duration: 1,
                x: outerEarX,
                y: -outerEarY,
                ease: "expo.out"
            });
            gsap.to(outerEarR, {
                duration: 1,
                x: outerEarX,
                y: outerEarY,
                ease: "expo.out"
            });
            gsap.to(earHairL, {
                duration: 1,
                x: -outerEarX,
                y: -outerEarY,
                ease: "expo.out"
            });
            gsap.to(earHairR, {
                duration: 1,
                x: -outerEarX,
                y: outerEarY,
                ease: "expo.out"
            });
            gsap.to(hair, {
                duration: 1,
                x: hairX,
                scaleY: hairS,
                transformOrigin: "center bottom",
                ease: "expo.out"
            });
        }
   

    function onEmailFocus(e: Event) {
        const passwordInput = document.getElementById('login__password') as HTMLInputElement;
        if (passwordInput.value === "" && (passwordInput.type === "text" || passwordInput.type === "password") && armL && armR) {
            uncoverEyes(armL, armR);
        }
        
        (e.target as HTMLElement).parentElement?.classList.add("focusWithText");

    }

    function onEmailBlur(e: Event) {
        if ((e.target as HTMLInputElement).value == "") {
            (e.target as HTMLElement).parentElement?.classList.remove("focusWithText");
        }
        resetFace();
    }

    function onPasswordFocus(e: Event) {
        if (!isPasswordVisible && armL && armR) {
            coverEyes(armL, armR);
        }
    }

    function onPasswordBlur(e: Event) {
        if (isPasswordVisible) {
            uncoverEyes(armL, armR);
        }
    }




    function resetFace() {
        gsap.to([eyeL, eyeR], {
            duration: 1,
            x: 0,
            y: 0,
            ease: "expo.out"
        });
        gsap.to(nose, {
            duration: 1,
            x: 0,
            y: 0,
            scaleX: 1,
            scaleY: 1,
            ease: "expo.out"
        });
        gsap.to(mouth, { 
            duration: 0.5, 
            scaleY: 1.5, 
            scaleX: 0.8, 
            repeat: 1, 
            yoyo: true,
            ease: "elastic.out" 
        });
        gsap.to(chin, {
            duration: 1,
            x: 0,
            y: 0,
            scaleY: 1,
            ease: "expo.out"
        });
        gsap.to([face, eyebrow], {
            duration: 1,
            x: 0,
            y: 0,
            skewX: 0,
            ease: "expo.out"
        });
        gsap.to([outerEarL, outerEarR, earHairL, earHairR, hair], {
            duration: 1,
            x: 0,
            y: 0,
            scaleY: 1,
            ease: "expo.out"
        });
    }

    function getAngle(x1: number, y1: number, x2: number, y2: number) {
        var angle = Math.atan2(y1 - y2, x1 - x2);
        return angle;
    }
    function uncoverEyes(armL: HTMLElement, armR: HTMLElement) {
        const passwordInput = document.getElementById('login__password') as HTMLInputElement | null;
        if (armL && armR && passwordInput?.type === 'password') {
            gsap.to([armL, armR], { // Animazione sincronizzata
                duration: 0.45,
                y: 220,
                ease: "quad.out",
                delay: 0.05
            });
            gsap.to(armL, { 
                rotation: 105, 
                duration: 0.45,
                ease: "quad.out",
                delay: 0.05
            });
            gsap.to(armR, { 
                rotation: -105, 
                duration: 0.45,
                ease: "back.out(1.7)",
                delay: 0.05
            });
        }
        else if (passwordInput?.type === 'text' && passwordInput.value !== '') {
            if (armL)
            {
                gsap.to(armL, { 
                    duration: 0.45,
                    y: 45,
                    ease: "back.out(1.7)",
                    delay: 0.05
                });
                getCoord();
    
            }
        }
        else if (passwordInput?.type === 'text' && passwordInput.value === '') {
            if (armL)
                {
                    gsap.to(armL, { 
                        rotation: 105, 
                        duration: 0.45,
                        ease: "quad.out",
                        delay: 0.05
                    });
                    resetFace();
        
                }
        }
    }

    function getPosition(el: HTMLElement) {
        var xPos = 0;
        var yPos = 0;

        while (el) {
            if (el.tagName == "BODY") {
                // deal with browser quirks with body/window/document and page scroll
                var xScroll = el.scrollLeft || document.documentElement.scrollLeft;
                var yScroll = el.scrollTop || document.documentElement.scrollTop;

                xPos += (el.offsetLeft - xScroll + el.clientLeft);
                yPos += (el.offsetTop - yScroll + el.clientTop);
            } else {
                // for all other non-BODY elements
                xPos += (el.offsetLeft - el.scrollLeft + el.clientLeft);
                yPos += (el.offsetTop - el.scrollTop + el.clientTop);
            }

            el = el.offsetParent as HTMLElement;
        }
        return {
            x: xPos,
            y: yPos
        };
    }

    email?.addEventListener('focus', onEmailFocus);
    email?.addEventListener('blur', onEmailBlur);
    email?.addEventListener('input', onEmailInput);
    password?.addEventListener('focus', onPasswordFocus);
    password?.addEventListener('blur', onPasswordBlur);
    const eyeIcon = `
<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-eye-fill" viewBox="0 0 16 16">
  <path d="M10.5 8a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0"/>
  <path d="M0 8s3-5.5 8-5.5S16 8 16 8s-3 5.5-8 5.5S0 8 0 8m8 3.5a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7"/>
</svg>`;
const eyeSlashIcon = `
<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-eye-slash-fill" viewBox="0 0 16 16">
  <path d="m10.79 12.912-1.614-1.615a3.5 3.5 0 0 1-4.474-4.474l-2.06-2.06C.938 6.278 0 8 0 8s3 5.5 8 5.5a7 7 0 0 0 2.79-.588M5.21 3.088A7 7 0 0 1 8 2.5c5 0 8 5.5 8 5.5s-.939 1.721-2.641 3.238l-2.062-2.062a3.5 3.5 0 0 0-4.474-4.474z"/>
  <path d="M5.525 7.646a2.5 2.5 0 0 0 2.829 2.829zm4.95.708-2.829-2.83a2.5 2.5 0 0 1 2.829 2.829zm3.171 6-12-12 .708-.708 12 12z"/>
</svg>`;

    const togglePasswordBtn = document.getElementById('togglePassword');
    
    if (togglePasswordBtn) {
        togglePasswordBtn.addEventListener('click', () => {
            isPasswordVisible = !isPasswordVisible;
            password.type = isPasswordVisible ? 'text' : 'password';
    
            // Cambia icona
            togglePasswordBtn.innerHTML = isPasswordVisible ? eyeIcon : eyeSlashIcon;
    
            // Animazioni occhi (facoltativo)
            if (isPasswordVisible) {
                uncoverEyes(armL, armR);
            } else {
                coverEyes(armL, armR);
            }
        });
    }
    
    gsap.set(armL, {
        x: -93,
        y: 220,
        rotation: 105,
        transformOrigin: "top left"
    });
    gsap.set(armR, {
        x: -93,
        y: 220,
        rotation: -105,
        transformOrigin: "top right"
    });
}

export function coverEyes(armL: HTMLElement, armR: HTMLElement) {
    if (armL && armR) {
        gsap.to([armL, armR], { // Animazione sincronizzata
            duration: 0.45,
            x: -93,
            y: 14,
            rotation: 0,
            ease: "quad.out",
            delay: 0.05 // Ritardo uguale per entrambe
        });
    }
}





  function setupPasswordToggle() {
    const togglePassword = document.getElementById('togglePassword');
    const eyeClosed = document.getElementById('eye-closed');
    const eyeOpen = document.getElementById('eye-open');
    let isPasswordVisible = false;
    const armL = document.querySelector('.armL');
    const armR = document.querySelector('.armR');
    const passwordInput = document.getElementById('login__password') as HTMLInputElement | null;
    if (togglePassword && passwordInput && eyeClosed && eyeOpen) {
        togglePassword.addEventListener('click', () => {
            isPasswordVisible = !isPasswordVisible;
            
            // Cambia tipo input
            passwordInput.type = isPasswordVisible ? 'text' : 'password';
            
            // Cambia icona
            if (isPasswordVisible) {
                eyeClosed.style.display = 'block';
                eyeOpen.style.display = 'none';
                // Braccio sinistro su, destro giù
                if (armL) gsap.to(armL, { duration: 0.5, y: 50, rotation: 0 });
                if (armR) gsap.to(armR, { duration: 0.5, y: 220, rotation: -105 });
            } else {
                eyeClosed.style.display = 'none';
                eyeOpen.style.display = 'block';
                // Riporta entrambe le braccia alla posizione originale
                if (armL) gsap.to(armL, { duration: 0.5, y: 220, rotation: 105 });
                if (armR) gsap.to(armR, { duration: 0.5, y: 220, rotation: -105 });
            }
        });
    }
}


export { initLoginAnimations };