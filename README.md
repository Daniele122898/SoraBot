## Major Changelogs (check frequent one with ´$changelog´)

**Version**
0.0.9.9.4

**Date**
19.3.2017 20:00 UTC +1

**Changes**
  - Better Image processing
  => weird characters used to break
	 the whole profile card.
  - Added Self-Assignable Roles!
  - A starboard entry that includes 
    any form of link will be posted 
    without the embed!
	  
	  
# Help => Can be found in Wiki as well (updated more frequently)

**GENERAL** If the Parameter has [ ] that means that it is optional. Leaving it will result in a different outcome.
For further questions join my Discord: https://discord.gg/Pah4yj5

## Dynamic Prefix

**GENERAL** What is a dynamic prefix? This basically means that you can choose the prefix that Sora shall use in your guild. The standart prefix is `$`!
Sora can always be invoked with mentioning him. But using that function is probably only usefull when setting or asking the prefix

| Command  | Parameter | Example                            | Permission    | Output                                                           |
|----------|-----------|------------------------------------|---------------|------------------------------------------------------------------|
| `prefix` | prefix    | `$prefix $$ / @Sora#7634 prefix $$` | Administrator | Changes the prefix of Sora in that guild to the specified prefix |
| `prefix` | *none*    | `$prefix / @Sora#7634 prefix`      | *none*        | Displays the current prefix of Sora in this guild                |

## Self-Assignable Roles
**GENERAL** Don't add any roles with high permissions in here or they `will` be abused by some angry member. Keep in mind that when you remove a role from the self-assignable roles list, someone that has the role cannot remove it from themselfs using the `$iamnot` command!

| Command       | Parameter    | Example            | Permission   | Output                                                                                                                                                                       |
|---------------|--------------|--------------------|--------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `addRole`     | Name of Role | `$addRole test`    | Manage Roles | Adds the specified role to the self assignable roles. Other users can then add them to themselfs using the bot                                                               |
| `removeRole`  | Name of Role | `$removeRole test` | Manage Roles | Removes the specified role from the self assignable roles. Keep in mind that afterwards users that have that role cannot remove it from themselfs using the $iamnot command! |
| `iam`         | Name of Role | `$iam test`        | *none*       | Adds the specified role to yourself                                                                                                                                          |
| `iamnot`      | Name of Role | `$iamnot test`     | *none*       | Removes the specified role from yourself                                                                                                                                     |
| `getRoles`    | *none*       | `$getRoles`        | *none*       | Posts a list of all self-assignable roles in the Guild                                                                                                                       |
| `getAllRoles` | *none*       | `$getAllRoles`     | *none*       | Posts a list of all roles in the Guild ordered by Position in the hirachy                                                                                                    |

## Searches

**GENERAL** With these you can make search queries for ub definitions, movies, series, animes and mangas!

| Command | Parameter                              | Example                        | Output                                                                                 |
|---------|----------------------------------------|--------------------------------|----------------------------------------------------------------------------------------|
| `imdb`  | Title of Movie / Series                | `$imdb Inception`              | Gives you data about the movie / series from IMDb like plot, rating, genres etc..      |
| `ub`    | Term to search for in Urban Dictionary | `$ub ohayou`                   | Gives you the definition of the Term with examples and its ub ratings                  |
| `anime` | Title of Anime Series / Movie          | `$anime Overlord`              | Gives you data about the Series / Movie from Anilist like plot, raiting, genres etc... |
| `manga` | Title of Manga                         | `$manga Boku no hero academia` | Gives you data about the Manga from Anilist like plot, raiting, genres etc...          |

## Music

**GENERAL** Sora can currently only play YT-Videos and no playlists. This will be added soon™ 
Newly added: Bot will leave the channel if no one is in it anymore

Command | Parameter | Example | Output 
--- | --- | --- | ---
`join` | *none* | `$join` | Joins the VoiceChannel of the calling user
`add` | YT video Url | `$add https://www.youtube.com/watch?v=Zt8Im_1fowA` | Only works with YT links. Adds the video to your queue
`skip` | *none* | `$skip` | Skips the current song and go to the next in the queue
`clear` | *none* | `$clear` | Clears the whole queue for the Guild. **Requires** Manage Channels permissions
`list` | *none* | `$list` | Shows the entire current queue
`np` | *none* | `$np` | Shows the current track that is playing
`play` | *none* | `$play` | Starts the playback of the current queue
`leave` | *none* | `$leave` | Leaves the channel of the user
`stop` | *none* | `$stop` | Stops the audio playback

## AFK / Away
**GENERAL** Keep in mind that your AFK status is GLOBAL. So if you want to say `i suck dick` in one guild but rather not in another then.. Do it at your own risk ;) I might add local / guild bound AFK in the future. The AFK message will only trigger every 30 seconds to prevent spam.

| Command | Parameter                                  | Example               | Output                                                                                                                                                                                              |
|---------|--------------------------------------------|-----------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `afk`   | [Optional message for when you get tagged] | `$afk not fapping...` | If you were set AFK before, the afk status gets removed. Otherwise will set you GLOBALY AFK on all guilds that Sora is on. Whenever you get @mentioned Sora will respond with your creative message |

## Custom Member Join / Leave Announcements

**ATTENTION** This system has changed and the Database has been wiped on 26.03.2017. 

**GENERAL** With this you can set custom leave and welcoming messages. These can be projected to different channels or independently turned off!

**IMPORTANT** In the messages themselfs you can use `{user}` to @Mention the user, ``{user#} for Name#Discriminator (mostly used in leave messages since the member can't be @Mentioned anymore), `{server}` for the server name, `{count}` for member count!
Example: `$a welcome Welcome {user} to {server} you are our {count}th member!`

**PREFIX** This command uses a module prefix. This means infront of all commands must be the prefix `a` or `announcement` !

| Command        | Parameter | Example                                                                  | Permission      | Output                                                                                                                 |
|----------------|-----------|--------------------------------------------------------------------------|-----------------|------------------------------------------------------------------------------------------------------------------------|
| `a welcome`    | [message] | `$a welcome Welcome {user} to {server} you are our {count}th member!`    | Manage Channels | Sets the Welcome channel to the current one and adds the Welcome message! => If left blank it will use the default one |
| `a welcomemsg` | [message] | `$a welcomemsg Welcome {user} to {server} you are our {count}th member!` | Manage Channels | Sets the custom Welcome message => If left blank it will use the default one                                           |
| `a welcomecha` | #channel  | `$a welcomecha #general`                                                 | Manage Channels | Sets the Welcome channel                                                                                               |
| `a rmwelcome`  | *none*    | `$a rmwelcome`                                                           | Manage Channels | Removes the whole Welcome announcement resetting the message aswell                                                    |
| `a leave`      | [message] | `$a leave {user} has sadly left us. RIP`                                 | Manage Channels | Sets the Leave channel to the current one and adds the Leave message! => If left blank it will use the default one     |
| `a leavemsg`   | [message] | `$a leavemsg {user} has sadly left us. RIP`                              | Manage Channels | Sets the custom Leave message => If left blank it will use the default one                                             |
| `a leavecha`   | #channel  | `$a welcomecha #general`                                                 | Manage Channels | Sets the Leave Channel                                                                                                 |
| `a rmleave`    | *none*    | `$a rmleave`                                                             | Manage Channels | Removes the whole Leave announcement resetting the message aswell                                                      |

## Tags

**ATTENTION** All of the following commands need a specified permission! If the `t restrict` command is not invoked no permissions are needed to create tags. Otherwise the set permissions are needed! The `t restrict` command needs **Administrator** permissions to be run.

**GENERAL** You cannot create a tag called `taglist` as it is a keyword. All tags will be created lowercase and called in lowercase no matter how you type them.

**PREFIX** This Command uses a module prefix. which means infront of all commands needs to be the prefix `tag` or `t` !

| Command      	| Parameter                	| Example                                  	| Output                                                                                                                                                                                          	|
|--------------	|--------------------------	|------------------------------------------	|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------	|
| `t create`   	| [tagname] │ [tagvalue]   	| `$t create test │ this will be displyed` 	| Creates a tag                                                                                                                                                                                   	|
| `t remove`   	| tagname                  	| `$t remove test`                         	| Removes the specified tag => You either need to be the creator or the specified permission group to do that. (If not specified it's manage hannels)                                             	|
| `t taglist`  	| *none*                   	| `$t taglist`                             	| This will list all tags in the Guild                                                                                                                                                            	|
| `t`          	| tagname                  	| `$t test`                                	| This will post the value of the specified tag                                                                                                                                                   	|
| `t restrict` 	| [permission to restrict] 	| `$t restrict Administrator`              	| Restricts the Tag Command to the specified permissions (If no permission is entered the restriction will be removed!) => `ManageChannels , Administrator, KickMembers, BanMembers, ManageGuild` 	|

## Starboard

**GENERAL** The starboard is a community based pin. What does that mean?
When someone writes something funny you can react to it with :star: or :star2: . This will post it in the specified Starboard channel (if not specified nothing will be posted)!
I recommend creating a channel called **Starboard** and restricting everyone from writing in it but able to read it. That way no one can temper with the quotes but everyone can enjoy them! When activated, Sora MUST have the permission to read the message history, otherwise he wont be able to donwload messages that he did not see to upload them to the Starboard!
You cannot star your own message. It will not be added to the starboard. Only different users can do that. When all reactions are deleted from the message the entry in the starboard will vanish aswell. The same message can only be added and removed twice, then it will be locked for the starboard and reactions to it will be blocked.

| Command      | Parameter | Example       | Permission      | Output                                                        |
|--------------|-----------|---------------|-----------------|---------------------------------------------------------------|
| `star`       | *none*    | `$star`       | Manage Channels | Sets the Channel in wich the command was invoked as Starboard |
| `starremove` | *none*    | `$starremove` | Manage Channels | Removes the starboard form the guild                          |

## Profile and EP

**PREFIX** This Command uses a module prefix. which means infront of all commands needs to be the prefix `p`!
**GENERAL** You will gain EP by writing messages... duh.

| Command       | Parameter       | Example                                           | Output                                                                                                                                  |
|---------------|-----------------|---------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------|
| `p`           | [@Mention user] | `$p / $p @Serenity#0783`                          | Creates a profile image showing the EP and Level of the User (If specified the mentioned user otherwise the invoking user)              |
| `p subscribe` | *none*          | `$p subscribe`                                    | Toggles your lvl up notifies (If Sora will Message you when you level up. Standard : false)                                             |
| `p top10`     | *none*          | `$p top10`                                        | Posts the top 10 list of users sorted by EP => The EP is globaly aquired on all Guilds that Sora is on!                                 |
| `p setbg`     | [URL to Image]  | `$p setbg www.example.com/image.jpg` / `$p setbg` | Not specifing the URL will remove your set BG and reset to the default profile card. This feature requires you to have atleast lvl 20!  |

## Help

| Command | Parameter    | Example            | Output                                                                                                                                       |
|---------|--------------|--------------------|----------------------------------------------------------------------------------------------------------------------------------------------|
| `help`  | *none*       | `$help`            | Posts a link to this help page.. duh.                                                                                                        |
| `help`  | command name | `$help t restrict` | Posts the indepth help of the specified command. Also shows all the possible aliases => usefull since i use alot of shortened names that way |

## Info

**PREFIX** This Command uses a module prefix. which means infront of all commands needs to be the prefix `info`!

| Command      | Parameter       | Example                                    | Output                                                                                                                                                                    |
|--------------|-----------------|--------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `info`       | *none*          | `$info`                                    | Displays all the infos about Sora. (Current Guilds, Architecture, uptime, ping etc..)                                                                                     |
| `info user`  | [@mention user] | `$info user / $info user @Serenity#0783` | If no user is entered as Parameter, you yourself will be the user displayed. This will post infos about the user like a link to his avatar, when he created the acc etc.. |
| `info guild` | *none*          | `$info guild`                                | Posts info about the guild similar to the user command                                                                                                                    |

## Miscellaneous and Fun

| Command    | Parameter       | Example                | Output                                                                            |
|------------|-----------------|------------------------|-----------------------------------------------------------------------------------|
| `say`      | message to echo | `$say i love this bot` | Will echo the parameter                                                           |
| `git`      | *none*          | `$git`                 | Will post a link to my gitlab page                                                |
| `feedback` | *none*          | `$feedback / $bug`       | Posts a link to my discord in where you can report bugs or issue Feature requests |
| `door`     | @mention        | `$door @Serenity#0783` | Shows the specified user the door. Mostly used after bad jokes                    |
| `lenny`    | *none*          | `$lenny`               | Posts a nice lenny face ( ͡° ͜ʖ ͡°)                                                  |
| `google`   | What to google  | `$google what is c#?`  | Googles something for you. Mostly used to google for others...                    |
| `swag`     | *none*          | `$swag`                | Swags the chat                                                                    |
| `about`    | *none*          | `$about`               | Gives infos about Sora and where he came from :)                                  |
| `ping`     | *none*          | `$ping`                | Shows the current ping of Sora                                                    |
| `invite`   | *none*          | `$invite`              | Posts a link to invite Sora to your guild :> plz ♥                                |

## Pats, Hugs and Pokes

| Command    | Parameter                | Example                                  | Output                                                                                                                  |
|------------|--------------------------|------------------------------------------|-------------------------------------------------------------------------------------------------------------------------|
| `pat`      | @mention of user to pat  | `$pat @Serenity#0783`                    | Will pat the specified user and add it to his global patcount. Spread love ♥ or pats :>                                 |
| `patcount` | [@mention]               | `$patcount` / `$patcount @Serenity#0783` | If no user is given as parameter you yourself will be displayed. Posts the global amount of pats the user has received. |
| `hug`      | @mention of user to hug  | `$hug @Serenity#0783`                    | Hugs the specified user with a randomly chosen hug gif                                                                  |
| `poke`     | @mention of user to poke | `$poke @Serenity#0783`                   | Pokes the specified user with a randomly chosen poke gif                                                                |


## Changelog

| Command     | Parameter | Example      | Output                            |
|-------------|-----------|--------------|-----------------------------------|
| `changelog` | *none*    | `$changelog` | See what the last update changed! |