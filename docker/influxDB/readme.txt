How to start container:
	docker-compose up -d // in folder where is docker-compose.yml located
How to kill and down container:
	docker-compose kill && docker-compose down

After kill and down container data are stored in folder data

How to init InfluxDB
	docker-compose exec -it influxdb /bin/bash
	influx setup

Configuration:

	user: influx-user
	password: password
	organization: TEST_ORG
	bucket: TEST_DATA
	retention: 0
	
	See into config/influx-configs
	
	
InfluxDB administration is http://localhost:8086/