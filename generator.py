import random
import string


hours_in_year = 8760


@staticmethod 
def random_string_generator(length): 
    return ''.join(random.choice(string.ascii_uppercase) for i in range(length))


record = 'TEST_DATA,'


for i in range(1000): 
    D1 = random_string_generator(4)
    D2 = random_string_generator(4)
    D3 = random_string_generator(4)
    D4 = random_string_generator(4)
    AUTHOR = random_string_generator(10)
    timestamp = 1640991600
    for j in range(8760):
       VALUE = random.uniform(0,1000)
       record = 'D1=' + D1 + ',' + 'D2=' + D2 + ',' + 'D3=' + D1 + ','+ 'D4=' + D4 + ' ' + 'AUTHOR=' + AUTHOR + ',' 'VALUE=' + str(VALUE) + ',' + 'CORRECTED_VALUE=' + str(round(VALUE, 2)) + ' ' + str(timestamp)
       timestamp = timestamp + 3600
       with open('soubor.txt', 'a') as soubor:
            soubor.write(record + '\n')