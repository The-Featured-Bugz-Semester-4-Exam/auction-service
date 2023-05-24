#/usr/bin/bash
export server="localhost"
export port="27017"
export auctionActiveCol="auctionActiveCol"
export auctionDoneCol="auctionDoneCol"
export database="Auction"
export redisConnection="localhost"
echo $database $auctionActiveCol
dotnet run server="$server" port="$port" auctionActiveCol="$auctionActiveCol" auctionDoneCol="$auctionDoneCol" database="$database" redisConnection="$redisConnection"