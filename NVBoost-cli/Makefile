
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
	mkdir -p bin/Release/deb/NVBoost-cli_1.0.0-1_amd64/usr/local/bin
	mkdir -p bin/Release/deb/NVBoost-cli_1.0.0-1_amd64/usr/share/NVBoost-cli
	mkdir -p bin/Release/deb/NVBoost-cli_1.0.0-1_amd64/DEBIAN
	echo 'Package: NVBoost-cli\nVersion: 1.0.0\nDepends: dotnet-runtime-9.0\nArchitecture: amd64\nDescription: A CLI tool to manage NVidia GPUs overclock, fans and power limit. Supports both Wayland and X11' > bin/Release/deb/NVBoost-cli_1.0.0-1_amd64/DEBIAN/control
	
	cp -r bin/Release/net9.0/publish/* bin/Release/deb/NVBoost-cli_1.0.0-1_amd64/usr/share/NVBoost-cli
	ln -sf /usr/share/NVBoost-cli/NVBoost-cli bin/Release/deb/NVBoost-cli_1.0.0-1_amd64/usr/local/bin/nvboost

	dpkg-deb --build --root-owner-group bin/Release/deb/NVBoost-cli_1.0.0-1_amd64

install:
	mkdir -p /usr/share/NVBoost-cli
	cp -r bin/Release/net9.0/publish/* /usr/share/NVBoost-cli

	ln -s /usr/share/NVBoost-cli/NVBoost-cli /usr/local/bin/nvboost
		
uninstall:
	rm -r /usr/share/NVBoost-cli
	rm /usr/local/bin/nvboost