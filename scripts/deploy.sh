bundtbotfile="bundtbot.tar.gz"
destinationFolder="bundtbot"
executable="BundtBot.dll"

user=root
host=""
#read -p "User: " user
#read -p "Host: " host

tar czvf $bundtbotfile -C src/BundtBot/bin/release/netcoreapp1.0/publish/ .
scp $bundtbotfile $user@$host:
ssh $user@$host "rm -rf $destinationFolder;
mkdir $destinationFolder;
tar xzvf $bundtbotfile -C $destinationFolder;
chmod +x $destinationFolder/$executable;
dotnet $destinationFolder/$executable"

echo "Press any key to exit"
read -n 1
