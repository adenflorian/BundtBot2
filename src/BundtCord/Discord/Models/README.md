# Questions
Should models contain references to things or their snowflake ids?

If you hold a reference, then if the thing being referenced goes away you now have a bad reference

if you hold the ids and check the master dictionaries everytimne you need a reference, then you will know when it's gone, and you only have to maintain the master dictionaries

Models of Discord objects (Text Channels, Users, Servers, etc.) **should never** store a reference to another discord model object.
Just store the id instead and check the master dictionaries in the discord client when you want a reference.
They may store a reference to the Discord Client, and the Server class may store a reference to a DiscordVoiceClient.

It may store a reference to something that will never go away (DiscordClient), or if it's something that it has the only reference (DiscordVoiceClient).