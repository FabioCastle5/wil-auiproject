import string
import re
import math

def low_pass_filter (a, a0, factor):
    return (a * factor + (1 - factor) * a0)

filename = "./../arduino/will_measurement_sketch/measure2_ypos.txt"
# open file with data and plot them
in_file = open(filename,"r")
x_list = []
filt_x_list = []
y_list = []
filt_y_list = []

threshold = 0.1
filter_factor = 0.65

entry = in_file.readline()
while len(entry) > 1:
    if entry.startswith("Ax"):
        [x,y] = re.findall("[-+]?[.]?[\d]+(?:,\d\d\d)*[\.]?\d*(?:[eE][-+]?\d+)?", entry)
        x_list.append(float(x))
        y_list.append(float(y))
    entry = in_file.readline()
in_file.close()

# evaluate the mean value
sumx = 0
sumy = 0
for i in range (len(x_list)):
    sumx = sumx + x_list[i]
    sumy = sumy + y_list[i]
avgx = sumx / len(x_list)
avgy = sumy / len(y_list)

# subtract mean value of the error
for i in range (len(x_list)):
    x_list[i] = x_list[i] - avgx
    y_list[i] = y_list[i] - avgy

# filter data
filt_x_list.append(low_pass_filter(x_list[0], 0, filter_factor))
filt_y_list.append(low_pass_filter(y_list[0], 0, filter_factor))
for i in xrange(1, len(x_list)):
    filt_x_list.append(low_pass_filter(x_list[i], x_list[i-1], filter_factor))
    filt_y_list.append(low_pass_filter(y_list[i], y_list[i-1], filter_factor))

# evaluate the quadratic mean value
sumx = 0
sumy = 0
for i in range (len(x_list)):
    sumx = sumx + x_list[i]
    sumy = sumy + y_list[i]
avgx = sumx / len(x_list)
avgy = sumy / len(y_list)

print ("MEAN DISPERISION FOR X: " + str(avgx))
print ("MEAN DISPERISION FOR Y: " + str(avgy))


# # make 0/1 decision
# for i in range(len(x_list)):
#     if x_list[i] > threshold:
#         x_list[i] = 1
#     elif x_list[i] < - threshold:
#         x_list[i] = -1
#     else:
#         x_list[i] = 0
# for j in range(len(y_list)):
#     if y_list[j] > threshold:
#         y_list[j] = 1
#     elif y_list[j] < - threshold:
#         y_list[j] = -1
#     else:
#         y_list[j] = 0
# for i in range(len(filt_x_list)):
#     if filt_x_list[i] > threshold:
#         filt_x_list[i] = 1
#     elif filt_x_list[i] < - threshold:
#         filt_x_list[i] = -1
#     else:
#         filt_x_list[i] = 0
# for j in range(len(filt_y_list)):
#     if filt_y_list[j] > threshold:
#         filt_y_list[j] = 1
#     elif filt_y_list[j] < - threshold:
#         filt_y_list[j] = -1
#     else:
#         filt_y_list[j] = 0
