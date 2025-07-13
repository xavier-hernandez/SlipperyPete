# SlipperyPete

**How to run**

Create a file (e.g. targets.txt) with lines like:
192.168.1.100:223
example.com
10.0.0.5:2200

The app will loop through each host:port, detect SSH banners, probe auth methods, and print Allowed/ Not allowed for password, public-key, and keyboard-interactive. 

If no port is provided then it checks port 22.

SlipperyPete.exe D:\temp\targets.txt


```bash
[192.168.1.100:223] SSH detected
Password authentication:        Allowed
Public-key authentication:      Allowed
Keyboard-interactive auth:      Allowed

[example.com:22] SSH detected
Password authentication:        Not allowed
Public-key authentication:      Allowed
Keyboard-interactive auth:      Not allowed

[10.0.0.5:2200] SSH detected
Password authentication:        Not allowed
Public-key authentication:      Allowed
Keyboard-interactive auth:      Not allowed
```
