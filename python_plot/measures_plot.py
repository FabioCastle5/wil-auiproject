import matplotlib.pyplot as plt
import matplotlib.gridspec as gridspec
import numpy as np
from scipy.interpolate import interp1d
import string
import re
import math
from collections import OrderedDict

# open file with data and plot them
in_file = open("./../arduino/will_measurement_sketch/measure_xnegfilt.txt","r")
x_list = []
y_list = []

# prepare a figure for nPlot x 1 plots
nPlot = 2
plot = 0
figure = plt.figure()
gs = gridspec.GridSpec(nPlot, 1)


entry = in_file.readline()
while len(entry) > 1:
    if entry.startswith("Ax"):
        [x,y] = re.findall("[-+]?[.]?[\d]+(?:,\d\d\d)*[\.]?\d*(?:[eE][-+]?\d+)?", entry)
        x_list.append(float(x))
        y_list.append(float(y))
    entry = in_file.readline()
in_file.close()

t = xrange(1, len(x_list) + 1)

# the first plot shows the ax data
ax1 = figure.add_subplot(gs[plot])
ax1.plot(t, x_list, 'ro', t, x_list, 'r-')
plot = plot + 1
k = 1
for x, y in zip(t, x_list):
    ax1.annotate(
        'point{0}'.format(k),
        xy=(x, y), xytext=(-5, 5),
        textcoords='offset points', ha='right', va='bottom',
        bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.5),
    )
    k = k + 1

# the second plot shows the ay data
ax2 = figure.add_subplot(gs[plot])
ax2.plot(t, y_list, 'bo', t, y_list, 'b-')
plot = plot + 1
k = 1
for x, y in zip(t, y_list):
    ax2.annotate(
        'point{0}'.format(k),
        xy=(x, y), xytext=(-5, 5),
        textcoords='offset points', ha='right', va='bottom',
        bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.5),
    )
    k = k + 1

plt.show()
