installcli:
	make -C NVBoost-cli publish
	make -C NVBoost-cli install


installgui:
	make -C NVBoost publish
	make -C NVBoost install

uninstallcli:
	make -C NVBoost-cli uninstall

uninstallgui:
	make -C NVBoost uninstall


installall: installcli installgui

uninstallall: uninstallgui uninstallcli

reinstallall: uninstallcli installgui

deb:
	make -C NVBoost-cli deb
	make -C NVBoost deb
