from datetime import datetime, timedelta
import pytz
import random
import string


hours_in_year = 8760

#function for generating random strings
@staticmethod 
def random_string_generator(length): 
    return ''.join(random.choice(string.ascii_uppercase) for i in range(length))

#Build date TIMESTAMPTZ for timescale
timestamp_timescale = datetime(2022,1,1,0,0,0)
timezone = pytz.timezone('Europe/Prague')
timestamp_timescale = timezone.localize(timestamp_timescale)


for i in range(100): 
    D1 = random_string_generator(4)
    D2 = random_string_generator(4)
    D3 = random_string_generator(4)
    D4 = random_string_generator(4)
    AUTHOR = random_string_generator(10)
    timestamp = 1640991600
    for j in range(100):
       #random value generation
       VALUE = random.uniform(0,1000)
       
       #influx part
       record_influx = 'TEST_DATA,' + 'D1=' + D1 + ',' + 'D2=' + D2 + ',' + 'D3=' + D3 + ','+ 'D4=' + D4 + ' ' + 'AUTHOR=' + AUTHOR + ',' 'VALUE=' + str(VALUE) + ',' + 'CORRECTED_VALUE=' + str(round(VALUE, 2)) + ' ' + str(timestamp)
       timestamp = timestamp + 3600
       
       #timescale part
       timestamp_str = timestamp_timescale.strftime("%Y-%m-%d %H:%M:%S.%f%z")
       record_timescale = timestamp_str + ',' + D1 + ',' + D2 + ',' + D3 + ',' + D4 + ',' + AUTHOR + ',' + str(VALUE) + ',' + str(round(VALUE, 2))
       timestamp_timescale += timedelta(hours=1)

       #writing to files
       with open('data_influx.csv', 'a') as soubor:
            soubor.write(record_influx + '\n')
       with open('data_timescale.csv', 'a') as soubor:
           soubor.write(record_timescale + '\n')