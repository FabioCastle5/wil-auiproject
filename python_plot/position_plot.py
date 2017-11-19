import matplotlib.pyplot as plt
import matplotlib.gridspec as gridspec
import numpy as np
from scipy.interpolate import interp1d
import string
import re
import math
from collections import OrderedDict


# open file with data and plot them
in_file = open("./xy_measures15.txt","r")
x_list = []
y_list = []

# prepare a figure for nPlot x 1 plots
nPlot = 3
plot = 0
figure = plt.figure()
gs = gridspec.GridSpec(nPlot, 1)


entry = in_file.readline()
while len(entry) > 1:
    if entry.startswith("x"):
        [x,y] = re.findall("[-+]?[.]?[\d]+(?:,\d\d\d)*[\.]?\d*(?:[eE][-+]?\d+)?", entry)
        x_list.append(np.floor(float(x)))
        y_list.append(np.floor(float(y)))
    entry = in_file.readline()
in_file.close()


# the first plot shows the raw data, without any compensations
ax1 = figure.add_subplot(gs[plot])
ax1.plot(x_list, y_list, 'ro', x_list, y_list, 'r-')
plot = plot + 1
seen = []
k = 1
for x, y in zip(x_list, y_list):
    if [x, y] not in seen:
        seen.append([x, y])
        ax1.annotate(
            'point{0}'.format(k),
            xy=(x, y), xytext=(-20, 20),
            textcoords='offset points', ha='right', va='bottom',
            bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.5),
        )
        k = k + 1


# remove the directions that won't be recognized by the videogame up to 45 degrees
r_indexes = []
max_angle = 90
j = 1
# first direction in between p0 and p1
a0 = math.degrees(math.pi + math.atan2(y_list[1]-y_list[0], x_list[1]-x_list[0]))
for i in xrange(2, len(x_list) - 1):
    # evaluate the angle respect to the previous point
    a = math.degrees(math.pi + math.atan2(y_list[i]-y_list[j], x_list[i]-x_list[j]))
    # if the angle is too high, that point will be deleted
    if abs(a - a0) > max_angle:
        r_indexes.append(i)
    else:
        a0 = a
        j = i

for i in reversed(r_indexes):
    x_list.pop(i)
    y_list.pop(i)

# the second plot shows the data with the elimination of too wide change in direction
ax2 = figure.add_subplot(gs[plot])
ax2.plot(x_list, y_list, 'ro', x_list, y_list, 'r-')
plot = plot + 1
seen = []
k = 1
for x, y in zip(x_list, y_list):
    if [x, y] not in seen:
        seen.append([x, y])
        ax2.annotate(
            'point{0}'.format(k),
            xy=(x, y), xytext=(-20, 20),
            textcoords='offset points', ha='right', va='bottom',
            bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.5),
        )
        k = k + 1


# delete points with false change in directions
r_indexes = []

for i in xrange(1, len(x_list) - 1):
    if (
    (x_list[i-1] < x_list[i+1] and x_list[i] < x_list[i-1]) or
    (x_list[i-1] > x_list[i+1] and x_list[i] > x_list[i-1]) or
    (y_list[i-1] < y_list[i+1] and y_list[i] < y_list[i-1]) or
    (y_list[i-1] < y_list[i+1] and y_list[i] < y_list[i-1])
    ):
        r_indexes.append(i)

for i in reversed(r_indexes):
    x_list.pop(i)
    y_list.pop(i)

# the third plot shows the data with the elimination of wrong change in direction
ax3 = figure.add_subplot(gs[plot])
ax3.plot(x_list, y_list, 'ro', x_list, y_list, 'r-')
plot = plot + 1
seen = []
k = 1
for x, y in zip(x_list, y_list):
    if [x, y] not in seen:
        seen.append([x, y])
        ax3.annotate(
            'point{0}'.format(k),
            xy=(x, y), xytext=(-20, 20),
            textcoords='offset points', ha='right', va='bottom',
            bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.5),
        )
        k = k + 1

# scale points to emulate the final result as the game will elaborate the circuit
scale = 1
for i in range(len(x_list)):
    if x_list[i] >= 0:
        x_list[i] = np.floor(x_list[i] / scale)
    else:
        x_list[i] = np.ceil(x_list[i] / scale)
    if y_list[i] >= 0:
        y_list[i] = np.floor(y_list[i] / scale)
    else:

        y_list[i] = np.ceil(y_list[i] / scale)

plt.show()
