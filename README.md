# You Are An Idiot Remade
The remake of the You are an idiot prankware.
# How to run
In order to run this you need to have dotnet framework version 4.7.2 or higher,
Make sure your antivirus is off otherwise it will delete the exe. 
Run the YouAreAnIdiot.exe as administrator this is because it modifies your registry in order to disable tskmgr.

# Will this hurt my pc?
No. It will not damage, steal, or delete/modify anything other than your registry.

# How do I re-enable task manager?
Open up regedit, then go to: Computer\HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System and set the "DisableTskMgr" dword to 0 instead of 1.

# How do I stop it from opening on startup?
Press WIN + R and type: "%appdata%" and press ok/enter,
Go to Microsoft->Windows->Start Menu->Programs->Startup and delete "YouAreAnIdiot"
