import string
import re

def low_pass_filter (a, a0, factor):
    return (a * factor + (1 - factor) * a0)

def defilter(af, af0, factor):
    return ((af - (1 - factor) * af0) / factor)

filename = "./../arduino/will_measurement_sketch/measure_xnegfilt.txt"
newfile = "./../arduino/will_measurement_sketch/measure1_xneg.txt"

# open file with data and plot them
in_file = open(filename,"r")
out_file = open(newfile, "w")
x_list = []
filt_x_list = []
y_list = []
filt_y_list = []

filter_factor = 0.8

entry = in_file.readline()
while len(entry) > 1:
    if entry.startswith("Ax"):
        [x,y] = re.findall("[-+]?[.]?[\d]+(?:,\d\d\d)*[\.]?\d*(?:[eE][-+]?\d+)?", entry)
        filt_x_list.append(float(x))
        filt_y_list.append(float(y))
    entry = in_file.readline()
in_file.close()

# defilter data
x_list.append(defilter(filt_x_list[0], 0, filter_factor))
y_list.append(defilter(filt_y_list[0], 0, filter_factor))
for i in xrange(1, len(filt_x_list)):
    x_list.append(defilter(filt_x_list[i], filt_x_list[i-1], filter_factor))
    y_list.append(defilter(filt_y_list[i], filt_y_list[i-1], filter_factor))

for i in range(len(x_list)):
    out_file.write("MEASURE " + str(i + 1) + ":\n")
    out_file.write("Ax = " + str(x_list[i]))
    out_file.write("\t\t\t|\t\t\t")
    out_file.write("Ay = " + str(y_list[i]) + "\n")
    out_file.flush()
out_file.close()
