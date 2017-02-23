**GENERAL** If the Parameter has [ ] that means that it is optional. Leaving it will result in a different outcome.
For further questions join my Discord: https://discord.gg/Pah4yj5

## Dynamic Prefix

**GENERAL** What is a dynamic prefix? This basically means that you can choose the prefix that Sora shall use in your guild. The standart prefix is `$`!
Sora can always be invoked with mentioning him. But using that function is probably only usefull when setting or asking the prefix

| Command  | Parameter | Example                            | Permission    | Output                                                           |
|----------|-----------|------------------------------------|---------------|------------------------------------------------------------------|
| `prefix` | prefix    | `$prefix $$ / @Sora#7634 prefix $$` | Administrator | Changes the prefix of Sora in that guild to the specified prefix |
| `prefix` | *none*    | `$prefix / @Sora#7634 prefix`      | *none*        | Displays the current prefix of Sora in this guild                |

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

## Announcements

Command | Parameter | Permission | Output 
--- | --- | --- | ---
`here` | *none* | Requires Mange Channel Permissions | Sets the channel in which the message was written as channel to announce joining or leaving members
`remove` | *none* | Requires Mange Channel Permissions | Removes the channel to announce joining or leaving members

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

| Command       | Parameter       | Example                  | Output                                                                                                                     |
|---------------|-----------------|--------------------------|----------------------------------------------------------------------------------------------------------------------------|
| `p`           | [@Mention user] | `$p / $p @Serenity#0783` | Creates a profile image showing the EP and Level of the User (If specified the mentioned user otherwise the invoking user) |
| `p subscribe` | *none*          | `$p subscribe`           | Toggles your lvl up notifies (If Sora will Message you when you level up. Standard : false)                                |
| `p top10`     | *none*          | `$p top10`               | Posts the top 10 list of users sorted by EP => The EP is globaly aquired on all Guilds that Sora is on!                    |

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

## Pats

| Command    | Parameter               | Example                                  | Output                                                                                                                  |
|------------|-------------------------|------------------------------------------|-------------------------------------------------------------------------------------------------------------------------|
| `pat`      | @mention of user to pat | `$pat @Serenity#0783`                    | Will pat the specified user and add it to his global patcount. Spread love ♥ or pats :>                                 |
| `patcount` | [@mention]              | `$patcount` / `$patcount @Serenity#0783` | If no user is given as parameter you yourself will be displayed. Posts the global amount of pats the user has received. |


## Changelog

| Command     | Parameter | Example      | Output                            |
|-------------|-----------|--------------|-----------------------------------|
| `changelog` | *none*    | `$changelog` | See what the last update changed! |