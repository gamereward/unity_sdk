================================
GETING START FOR UNITY DEVELOPER
================================
*****************
Register Account:
*****************
Go to gamereward portal: https://gamereward.io/ and open the menu for developer.

Register account with your email and password.

Login to system after go to your email to active account.

*******************
Create Application:
*******************

Open tap application in the menu.

Click the “Add new” button.

Fill your application name.

Select application type: “Manage users”.

Fill the application description.

Click the “Save” button.

***********************
Write your server code:
***********************
Click to your application in the application list to view the application detail.

Click to button scripts.

Click to button Add new in the script list.

Fill the scriptname field.

Put your server script the script content.

Click Save button

*************************
Server script programing:
*************************
+Script style: 
==============

GameReward server script allow javascript style, so all the javascript types which are not browser object are accepted.

+Access modifiers: 
==================
-private: 
---------
a function with private modifier or not define modifier can not call from client sdk, just be called from another function in the same script.

-public:
-------- 
a function with public modifier can call from client sdk. If a function is not defined public and call from client will return error function does not exists or it’s not a public function.

-Example:
---------

A function that return a random card in 52 playing cards:

.. code-block:: js

  // private function
  function getCard(symbol,suit){
	//Return the card object
	return {"symbol": symbol, "suit":suit};
  }
  public function random_number(){
  //Random a symbol
	var symbol=Math.round(Math.random()*12)+2; 
	var suit=Math.round(Math.random()*4); 
	return getCard(symbol,suit);
  }

+Predefined Function:
=====================
-Wallet functions:
------------------
**chargeMoney(value)**: This function use for charge money or pay money to user. Function return true if action is successful otherwise return false.

If value > 0 the function will charge money from user wallet and transfer to application wallet.

If value <0 the function will transfer money from application wallet to user wallet.

**getBalance()**: This function use to get the balance of coin in the user wallet.

**Example:** A function that allow bet the number from 1 to 5:

.. code-block:: js

  public function bet_number(number,bet){
	//Get random number
	var rand_number=Math.round(Math.random()*4)+1;
	if(bet<0){
	    bet=-bet;//Not allow negative
	}
	if(rand_number == number){
	   // if the player win
	   chargeMoney(-4*bet);//Pay 4* bet coin to player
	}
	else{
	   chargeMoney(bet); // Charge bet coin from player
	}
	//Return an array contains 2 numbers
	return [number,rand_number];
  }

-Score functions:
-----------------
**getUserScore(scoretype)**: return the score of user in scoretype. Scoretype is a string that indicates the type of score.

**getUserRank(scoretype)**: return the rank of user order by score in scoretype.

**saveUserScore(scoretype,score)**: save the score of player by scoretype. 

**increaseUserScore(scoretype,score)**: add more score to player score in scoretype.

**Example:** increase the user score when player win:

.. code-block:: js

  public function bet_number(number,bet){
       var rand_number=Math.round(Math.random()*4)+1;
       if(rand_number==number){
            …
           //Increase the current score by the money player had bet.
           increaseUserScore("BET_NUMBER_SCORE",bet);
       }
  }

-Session Data functions: 
------------------------
GameReward allow store data by sessions. In each session data can save and get by key,value pair. Session store by storage define by programmer.

Switch to one session store by call function:

**setSessionStore(store);** //set the storage location of the session identify by parameter store

Start new session by calling function:

**newUserSession();**// create new session in current storage

To save session data by key by calling function:

**saveUserSessionData(key,data);** //save the data in current session

The data always override the old data in the same key in current session until create a new session.

To read the current session data by calling:

**loadUserSessionData(key);** //return string data saved in current session by key.

**Example:** Save the player data to history (this data can get by the sdk):

.. code-block:: js

  public function bet_number(isNewGame,number,bet){
        setSessionStore("BET_NUMBER_GAME");
        if(isNewGame){
            newUserSession();
        }
        var count= loadUserSessionData("count");
        if(count==null||count=="")
        {
            count=4;
        }
        else{
            count=parseInt(count);
        }
        var rand_number=Math.round(Math.random()*count)+1;
        if(rand_number==number){
           …
           count++;
           saveUserSessionData("count",count); // save the count number for next call
        }
        saveUserSessionData("data",[number,rand_number, count,bet]); //Save history
        return [number,rand_number, count,bet];
   }

************************
Download the client sdk:
************************
Open the tab Download in the developer portal.

Go to session Client SDK and select download the Unity sdk.

Extract the zip file and import the unitypackage to your unity project.

*************************
The Unity client SDK Api:
*************************

For more information please checkout the api description in the website:
https://test.gamereward.io/docs/sdk/v1.0/unity/index.html

+Init application parameters:
=============================

**GrdManager.Init(appId,secret,net);**

The appId and secret get from the details page of application in the developer portal.

net: GrdNet.TestNet for current because the main net is not release yet.

+Account actions:
=================

-Login to system by calling:
----------------------------
**GrdManager.Login(username,password,otp,callback);**

username,password are username and password to login

otp is Google Authenticator Otp Code need provide when player setting enable otp.

callback: is callback delegate call when data is responsed from server. Callback has two parameter:

   error: error code 

   args: data return.

**Example:**

.. code-block:: C#

  GrdManager.Login("suppergamer","123456","",(error,args)=>{
        if(error==0){
           //Login success
        }
        else if(error==4){
           // Correct username and password however otp is not correct
           // Show the otp screen
        }
        else{
            //Show the error message
            Debug.Log(args.ErrorMessage);
        }
  });

-Register new account by calling:
---------------------------------
**GrdManager.Register(username,password,email,userdata,callback);**

In the method:

Email use for verify account, reset password…

userdata: is a string that can be anything.

callback: is callback delegate call when data is responsed from server.

**Example:**

.. code-block:: C#

  GrdManager.Register("suppergamer","123456","suppergamer@gmail.com",(error,args)=>{
         if(error==0){
             //Register success
         }
         else{
              //Show the error message
              Debug.Log(args.ErrorMessage);
         }
  });

-Reset password by calling: 
---------------------------

//Send an email to resetpassword

**GrdManager.RequestResetPassword(email, callback);**

//Reset password by token from email to the newpassword

**GrdManager.ResetPassword(token,newpassword, callback);**

+In game actions:
=================

-Call the server script functions:
----------------------------------

**GrdManager.CallServerScript(scriptName,functionName, parameters, callback);**

scriptName: is the name of the script in the application in portal

functionName: is the name of the public function in application in portal

parameters: an array of the parameters pass to the function in order.

callback: delegate call when server response data.

**Example:**

Server scripts:

.. code-block:: js

  private function getRandom(start,end){
     return parseInt(Math.round(Math.random()*(end-start)+start));
  }
  public function random9(number,bet){
    var balance=getBalance();
    var price=parseFloat(bet);
    if(price<=0){
         return [1,'Wrong request!'];//1:Error
    }
    if(balance < price){
         return [1,'Player not enough money!'];//1:Error
    }
    var ranNumber=getRandom(1,9);
    var win=0;    
    var yournumber=parseInt(number);
    var score=getUserScore('random9_score');//Get the current score
    if(yournumber==ranNumber){
       win=5*price;
       if(!chargeMoney(-win)){
          return [2,'Game is out of cash!'];//2:Error game is not enough money for player
       }
       if(score==null){
          score=0;
       }
       score=score+5; //increase score
       saveUserScore('random9_score',score);
    }
    else{    
       win=-price;
       if(!chargeMoney(price)){
           return [1,'Player not enough money!'];//1:Error
       }  
       if(score==null){
           saveUserScore('random9_score',score);
       }
    }  
    setSessionStore('GAME-9');
    newUserSession();
    saveUserSessionData('rand',ranNumber+","+yournumber+","+win);
    return [0,ranNumber,yournumber,win,balance];//Success
  }

Client scripts:

.. code-block:: C#

  number=5;// In the game this number is the number player select in the UI
  bet=1;//The money player bet
  //Call the  random9 function with 2 parameters
  GrdManager.CallServerScript("TestScript","random9",new object[]{number, bet},(error,args){
           if(error==0){
               //No error when executing
               //Return an array in the random9
               List<object>ls=(List<object>) args.Data;
               if(ls[0].ToString()=="0"){
                   //The function exec successfully
                  
               }
               else{
                   // Error by logic game
                   Debug.Log(ls[1].ToString()); 
               }
           }
           else{		
                 //There was error by network
                  Debug.Log(args.ErrorMessage);
           }
   });

+Wallet actions:
================
-User information:
------------------
User information store in static property:

**GrdManager.User**

-Transfer:
----------
To transfer user coin to other wallet (withdraw money):

**GrdManager.Transfer(toAddress,money,otp,callback);**

**Example:**

.. code-block:: C#

  GrdManager.Transfer("0xafec……",100,"",(error,args)=>{
          if(error==0){
              //Transfer success
          }
          else if(error==4){
              // Need provide otp
              // Show the otp screen
          }
          else{
              //Show the error message
              Debug.Log(args.ErrorMessage);
          }
  });


-Update balance:
----------------
To update the latest balance of user wallet

**GrdManager.UpdateBalance();**

-Transaction:
-------------
Get the transactions list:

**GrdManager.GetTransactions(int start,int count,GrdTransactionEventHandler callback);**
