
clean:
	dotnet clean 

restore:
	dotnet restore

build: clean restore
	dotnet build --configuration Release

publish: build
	dotnet publish
	
run: build
	dotnet run --configuration Release

deb: publish
	mkdir -p bin/Release/deb/NVBoost_1.0.0-1_amd64/usr/local/bin
	mkdir -p bin/Release/deb/NVBoost_1.0.0-1_amd64/usr/share/applications
	mkdir -p bin/Release/deb/NVBoost_1.0.0-1_amd64/usr/share/NVBoost
	mkdir -p bin/Release/deb/NVBoost_1.0.0-1_amd64/DEBIAN
	echo 'Package: NVBoost\nVersion: 1.0.0\nDepends: dotnet-sdk-9.0\nArchitecture: amd64\nDescription: A GUI tool to manage NVidia GPUs overclock, fans and power limit. Supports both Wayland and X11' > bin/Release/deb/NVBoost_1.0.0-1_amd64/DEBIAN/control
	
	cp -r bin/Release/net9.0/linux-x64/publish/* bin/Release/deb/NVBoost_1.0.0-1_amd64/usr/share/NVBoost
	cp Assets/NVBoost.desktop bin/Release/deb/NVBoost_1.0.0-1_amd64/usr/share/applications/NVBoost.desktop
	ln -sf /usr/share/NVBoost/NVBoost bin/Release/deb/NVBoost_1.0.0-1_amd64/usr/local/bin/nvboost-gui

	dpkg-deb --build --root-owner-group bin/Release/deb/NVBoost_1.0.0-1_amd64

install:
	mkdir -p /usr/share/NVBoost
	mkdir -p /usr/share/applications/
	cp -r bin/Release/net9.0/linux-x64/publish/* /usr/share/NVBoost
	cp Assets/NVBoost.desktop /usr/share/applications/NVBoost.desktop

	ln -sf /usr/share/NVBoost/NVBoost /usr/local/bin/nvboost-gui
		
uninstall:
	rm -r /usr/share/NVBoost
	rm /usr/share/applications/NVBoost.desktop
	rm /usr/local/bin/nvboost-gui