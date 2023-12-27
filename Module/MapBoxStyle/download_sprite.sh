#https://api.mapbox.com/styles/v1/ke7789/ckvud88ah2v6515qsd4zu0uh4.html?title=view&access_token=pk.eyJ1Ijoia2U3Nzg5IiwiYSI6ImNrb28waGg3dzA1cHoydnMyOTB2ZmRtdGEifQ.kYLjwLwATw-4exWJQOIR7w&zoomwheel=true&fresh=true#16.7/37.5132/127.054789

#style_path="ke7789/ckvta2ylt1mq614s8vrp37ktq"
style_path="ke7789/ckvta2ylt1mq614s8vrp37ktq"
access_token="pk.eyJ1Ijoia2U3Nzg5IiwiYSI6ImNrb28waGg3dzA1cHoydnMyOTB2ZmRtdGEifQ.kYLjwLwATw-4exWJQOIR7w"


curl "https://api.mapbox.com/styles/v1/$style_path/sprite@3x.png?access_token=$access_token" -o mb_sprites.png
curl "https://api.mapbox.com/styles/v1/$style_path/sprite@3x.json?access_token=$access_token" -o mb_sprites_meta.json


