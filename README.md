# 🔧 Rust Server Manager
![Preview](RustServerManager/preview.png)

---

## 🚀 Features

- 🔥 **Rust-Themed UI** with molten glow and lava-styled panels
- 📈 **Live CPU, RAM usage charts**
- 💬 **RCON Console** built-in for sending commands and reading logs (Needs some help, it is in beta)
- 🎛️ **Plugin manager** to enable/disable and deploy (Oxide plugins not yet implemented)
- 📊 **Player stats tracking** (kills, uptime, inventory, and more) (Not yet implemented)
- ♻️ **Multi-instance server management**
- ⚙️ **Startup config, auto-restart, and backup support** (Auto Start and Auto Update not implemented yet)
- ☁️ **MySQL-backed server + player data**

---

## 📦 Installation

1. Clone the repo or download the latest [release](https://github.com/remathes/RustServerManager/releases)
2. Open `RustServerManager.sln` in Visual Studio 2022+
3. Build and run
4. Open `RustUpdate.sln` in Visual Studio 2022+
5. Build and run

Additional steps
1. To use the project as is install mysql 8.0.3 setup user accounts root,rustadmin
2. Create a database using the .sql scripts in the project
3. Edit the dbconfig.json with your mysql info
4. Run CreateDatabase.sql
5. Run TableInstances.sql and TableRust_Log.sql
6. Compile the RustServerManager project and the RustEdit project make sure to copy the /bin/release or /bin/debug
of the RustUpdate project to the RustServerManager /bin/release or /bin/debug basically RustUpdate.exe has to live where the RustServerManager.exe
is located.
7. Once up and running click the + sign to add an instance, fill out required information save.
8. You may have to restart the RustServerManager if it did not add the new instance right away (Bug have not fixed it yet). You should
be able to click play icon now, this shoud open the RustUpdate.exe where you can update the rust server and oxide for plugins from here you
should be all set for the first instance. You can add/edit more from the menu items as normal but may have to re-open to see them.

A free and open-source desktop tool to monitor, manage, and control Rust servers — with a modern forge-inspired UI and real-time data.

> Requires **.NET 8**

---

## 🧪 In Development

- 🔄 Remote control panel for web access
- 🌍 Live Rust map viewer
- 🎯 Plugin sandbox deployment
- 🔔 Discord alerts
- 🔒 Secure credentials encryption

---

## 🤝 Contributing

PRs welcome! Check out the [Projects tab](https://github.com/remathes/RustServerManager/projects) and Issues to get started.

---

## ☕ Support
- I am a novice program type person. I have messed around with
- code for 20 years but still don't understand a lot but I can
- make my way around to get things working. Also 75% of this was
- built from Chatgpt so if it looks odd/off it is because well its
- AI and my limited knowledge working together with concepts and
- ideas. Feel free do download mess around or what ever!
- ⭐ Star the repo
- 💬 Share it with other server admins

---

## 📜 License

MIT — free for personal and commercial use.
