# SmallTV Timer Screen

Miss the time display on newer Echo devices? Have a GeekMagic SmallTV device?

This project provides a service to display active Alexa timers on a GeekMagic SmallTV device, using its photo album display feature.

## Requirements

- A [Home Assistant](https://www.home-assistant.io/) instance with the [Alexa Media Player](https://github.com/alandtse/alexa_media_player) integration
- A server in your network that can host the TimerScreen service (e.g. some Raspberry Pi or similar)
- A GeekMagic SmallTV (e.g. on [AliExpress](https://www.aliexpress.com/item/1005006159850972.html)). Either Ultra (ESP8266) or Pro (ESP32) is fine.
  Tested on a SmallTV Ultra with firmware version 9.0.33.

### TimerScreen Server

#### Example Setup on a Linux server

1. Install [ASP.NET Core Runtime 8.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) on your server

2. Download the latest release of TimerScreen from the [releases page](https://github.com/3breadt/SmallTvTimerScreen/releases/)

3. Unpack to /var/www/smalltv-timerscreen and grant read access to the www-data user.
   This assumes a www-data user already exist. Adjust as necessary for your system.

4. Create a systemd service file `/etc/systemd/system/smalltv-timerscreen.service` with the following content (adjust paths as necessary):
   ```ini
   [Unit]
   Description=Displays Alexa Timers on a SmallTV device
   
   [Service]
   WorkingDirectory=/var/www/smalltv-timerscreen
   ExecStart=/usr/bin/dotnet /var/www/smalltv-timerscreen/SmallTvTimerScreen.dll
   Restart=always
   # Restart service after 10 seconds if the dotnet service crashes:
   RestartSec=10
   KillSignal=SIGINT
   SyslogIdentifier=dotnet-smalttv-timerscreen
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=DOTNET_NOLOGO=true
   Environment=ASPNETCORE_URLS=http://+:5000
   
   [Install]
   WantedBy=multi-user.target
   ```
   Adjust the value of ASPNETCORE_URLS according to which ports are available on your system.
   Open that port so that it is accessible by Home Assistant.
   You may also place a reverse proxy in front of the application server, e.g. nginx or Apache.

5. Reload the systemd configuration and start the service:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl start smalltv-timerscreen
   ```

### Home Assistant

1. Install the Alexa Media Player integration in Home Assistant and configure it with your Amazon account.
   A guide is available in the [Alexa Media Player integration wiki](https://github.com/alandtse/alexa_media_player/wiki/Configuration).

2. Add a [RESTful command](https://www.home-assistant.io/integrations/rest_command/) to your `configuration.yaml` to send active timers to the TimerScreen service:
   ```yaml
   rest_command:
     smalltv_timerscreen:
       url: 'http://<TIMER_SCREEN_SERVER>/timer/nexttimer'
       method: POST
       headers:
         accept: 'application/json'
        payload: '{{ states.sensor.echo_dot_next_timer.attributes | tojson }}'
       content_type: 'application/json; charset=utf-8'
   ```

   _Replace `<TIMER_SCREEN_SERVER>` with the actual IP address and port of your TimerScreen server_
  _Replace `sensor.echo_dot_next_timer` with the actual ID of the next timer entity in your Home Assistant instance_

3. Create an automation that triggers when the state of your Echo device's next timer changes and then sends the active timers to the TimerScreen service.

   ```yaml
   alias: Show Alexa Timer on SmallTV
   description: ""
   triggers:
     - trigger: state
       entity_id:
         - sensor.echo_dot_next_timer
   conditions: []
   actions:
     - action: rest_command.smalltv_timerscreen
   mode: single
   ```

   _Replace `sensor.echo_dot_next_timer` with the actual ID of the next timer entity in your Home Assistant instance_
