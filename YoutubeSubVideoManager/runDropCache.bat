set /p VIDEOID=<%localappdata%\YoutubeSubVideoManager\lastOpenedVideoId.txt
YoutubeSubVideoManager.exe --after-video-id=%VIDEOID% --video-count 50 --drop-cache