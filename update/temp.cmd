certutil -hashfile A37\old\ColorMC.Core.dll SHA1
certutil -hashfile A37\old\ColorMC.Gui.dll SHA1
hdiffz.exe -m-6 -SD -c-zstd-21-24 -d A37\old A37\new A37\out.hdiff
certutil -hashfile A37\out.hdiff SHA1
certutil -hashfile A37\new\ColorMC.Core.dll SHA1
certutil -hashfile A37\new\ColorMC.Gui.dll SHA1

{
    "diff": "0001.hdiff",
    "sha1": "a34fe7704cea6f536da26340f96652569fab675f",
    "core": "88fd838654229b003c03907dafe30c82fff10828",
    "gui": "e506c9577ac979b2247dfb9298f470abebc1294a"
}