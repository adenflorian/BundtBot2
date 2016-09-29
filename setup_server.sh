user=root
red=`tput setaf 1`
green=`tput setaf 2`
reset=`tput sgr0`

read -p "Enter host: " host

echo "${green}Logging in to $host as $user${reset}"

aptGetUpdateAndUpgrade='apt-get update; apt-get upgrade -y'
echo "${green}${aptGetUpdateAndUpgrade}${reset}"
ssh $user@$host $aptGetUpdateAndUpgrade
echodotnetdevSource='echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ xenial main" > /etc/apt/sources.list.d/dotnetdev.list;'
echo "${green}${echodotnetdevSource}${reset}"
ssh $user@$host $echodotnetdevSource
aptkeyrecv='apt-key adv --keyserver apt-mo.trafficmanager.net --recv-keys 417A0893;'
echo "${green}${aptkeyrecv}${reset}"
ssh $user@$host $aptkeyrecv
aptGetUpdate='apt-get update;'
echo "${green}${aptGetUpdate}${reset}"
ssh $user@$host $aptGetUpdate
installDotNet='apt-get install -y dotnet-dev-1.0.0-preview2-003131'
echo "${green}${installDotNet}${reset}"
ssh $user@$host $installDotNet

echo "${green}Done${reset}!"
read -n 1 -p "Press any key to exit..."
