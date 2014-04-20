Twitchbot
=========

A bot for twitch.tv chat
The bot requires 2 additional files to be created

rooms.txt which should contain a list of rooms to join on startup, like so:
```
#room1
#room2
#room3
```

user.txt which should contain username and passowrd, like so:
```
username
password
```

There are optional files to specify as well:

regulars.txt
```
#room1
message1
time between messages in minutes1
min messages before saying stuff1
#room2
message2
time between messages in minutes2
minmessages before saying stuff2
```


Commands
=========

##AdminCommands

#### !join channel
Makes bot join specified channel

#### !Shutdown !Exit
Makes bot shutdown cleanly, storing config data

## ModCommands

#### !AddReply cmdName reply
Adds the command !cmdName to reply with the given text

#### !DelReply cmdName
Removes the command

#### !test
Sends a test message to see if the bot is working

#### !part
Makes the bot leave the room

#### !Count
Shows the amount of messages since the bot has joined

## UserCommands

#### !Klappa
Kappa/

#### !Slap user
Slaps specified user

#### !Insult user
Insults specified user

#### !Info
Prints bot info