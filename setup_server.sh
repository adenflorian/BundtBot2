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

aptkeyrecv='apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893;'
echo "${green}${aptkeyrecv}${reset}"
ssh $user@$host $aptkeyrecv

aptGetUpdate='apt-get update;'
echo "${green}${aptGetUpdate}${reset}"
ssh $user@$host $aptGetUpdate

installDotNet='apt-get install -y dotnet-dev-1.0.0-preview2.1-003177'
echo "${green}${installDotNet}${reset}"
ssh $user@$host $installDotNet

installNginx='apt-get install -y nginx'
echo "${green}${installNginx}${reset}"
ssh $user@$host $installNginx

curlBundtBotSite='curl -o /etc/nginx/sites-available/bundtbot https://raw.githubusercontent.com/AdenFlorian/BundtBotBeta/master/nginx/sites/bundtbot'
echo "${green}${curlBundtBotSite}${reset}"
ssh $user@$host $curlBundtBotSite

startNginx='service nginx start'
echo "${green}${startNginx}${reset}"
ssh $user@$host $startNginx

echo "${green}Done${reset}!"
read -n 1 -p "Press any key to exit..."
