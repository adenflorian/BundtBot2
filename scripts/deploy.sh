bundtbotfile="bundtbot.tar.gz"
destinationFolder="bundtbot"
executable="BundtBot.dll"

user=$1
host=$2

echo 'packaging app'
tar czf $bundtbotfile -C src/BundtBot/bin/release/netcoreapp1.0/publish/ .

echo 'secure copying app'
scp $bundtbotfile $user@$host:

ssh $user@$host "
	echo 'stopping bundtbot service';
	service bundtbot stop;
	echo 'deleting old app';
	rm -rf $destinationFolder;
	mkdir $destinationFolder;
	echo 'unpacking new app';
	tar xzf $bundtbotfile -C $destinationFolder;
	chmod +x $destinationFolder/$executable;
	echo 'starting bundtbot service';
	service bundtbot start;
"

echo "Done."
