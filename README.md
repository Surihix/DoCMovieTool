# DoCMovieTool
This small tool allows you to unpack and repack the movie archive files from Dirge of Cerberus.

# Important notes
- This tool will only allow you to replace the existing files inside a movie archive.
- Do not rename the movie archive file or the unpacked folder. the tool uses the archive filename or the unpacked foldername to use an appropriate set of keys, that help in decrypting and encrypting the individual movie files.
- Do not repack any of the PAL region movie files into NTSC-U (USA region) or NTSC-J (Japan / Japan international region) version's archive files. the game is strict about the movie file's framerate during runtime and as the PAL region movie files are encoded
  at 25.000fps, the game will get stuck in a black screen when it tries playing this file. you can freely interchange movie files between NTSC-U and NTSC-J versions of the game as they all run in the same framerate.
