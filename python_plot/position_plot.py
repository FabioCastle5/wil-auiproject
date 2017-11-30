import matplotlib.pyplot as plt
import matplotlib.gridspec as gridspec
import numpy as np
from scipy.interpolate import interp1d
import string
import re
import math
from collections import OrderedDict

def round_zero(val):
    if val > 0:
        return np.floor(val)
    elif val < 0:
        return np.ceil(val)
    else:
        return val;

# open file with data and plot them
in_file = open("./xy_measures22_ypositivetest.txt","r")
x_list = []
y_list = []
circ_x = []
circ_y = []

# prepare a figure for nPlot x 1 plots
nPlot = 4
plot = 0
figure = plt.figure()
gs = gridspec.GridSpec(nPlot, 1)


entry = in_file.readline()
while len(entry) > 1:
    if entry.startswith("x"):
        [x,y] = re.findall("[-+]?[.]?[\d]+(?:,\d\d\d)*[\.]?\d*(?:[eE][-+]?\d+)?", entry)
        y_list.append(round_zero(float(y)))
        x_list.append(round_zero(float(x)))
    entry = in_file.readline()
in_file.close()

# remove the overlapping points
r_indexes = []
j = 0
for i in xrange(1, len(x_list) - 1):
    if (x_list[i] - x_list[j] == 0) and (y_list[i] - y_list[j] == 0):
        r_indexes.append(i)
    else:
        j = i

for i in reversed(r_indexes):
    x_list.pop(i)
    y_list.pop(i)

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
            xy=(x, y), xytext=(-5, 5),
            textcoords='offset points', ha='right', va='bottom',
            bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.5),
        )
        k = k + 1


# remove the directions that won't be recognized by the videogame up to 45 degrees
r_indexes = []
max_angle = 90
j = 1
# first direction in between p0 and p1
a0 = math.degrees(math.atan2(y_list[1]-y_list[0], x_list[1]-x_list[0]))
for i in xrange(2, len(x_list) - 1):
    # evaluate the angle respect to the previous point
    a = math.degrees(math.atan2(y_list[i]-y_list[j], x_list[i]-x_list[j]))
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
            xy=(x, y), xytext=(-5, 5),
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
            xy=(x, y), xytext=(-5, 5),
            textcoords='offset points', ha='right', va='bottom',
            bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.5),
        )
        k = k + 1

# # evaluate the tile dimension
# min_dist = math.hypot(x_list[1] - x_list[0], y_list[1] - y_list[0])
# max_dist = min_dist
# for i in xrange(2, len(x_list) - 1):
#     distance = math.hypot(x_list[i] - x_list[i-1], y_list[i] - y_list[i-1])
#     if distance < min_dist:
#         min_dist = distance
#     elif distance > max_dist:
#         max_dist = distance
# print(min_dist)
# print(max_dist)
# tileDimension = max_dist / min_dist / 2

# evaluate the mean distance between the points
# used to scale the circuit in a tile-like way
mean_dist = math.hypot(x_list[1] - x_list[0], y_list[1] - y_list[0])
for i in xrange(2, len(x_list)):
    distance = math.hypot(x_list[i] - x_list[i-1], y_list[i] - y_list[i-1])
    mean_dist = (distance + mean_dist * (i - 1)) / i
print("Mean distance: " + str(mean_dist))
min_distance = mean_dist

# scale points in a good way
j = 0
circ_x.append(x_list[0])
circ_y.append(y_list[0])
for i in xrange(1, len(x_list)):
    module = math.hypot(x_list[i] - x_list[j], y_list[i] - y_list[j])
    angle = math.degrees(math.atan2(y_list[i]-y_list[j], x_list[i]-x_list[j]))
    scale_module = round_zero(module / min_distance)
    if not (scale_module == 0):
        if math.fabs(angle) == 90:
            deltaX = 0
        else:
            deltaX = scale_module * math.cos(math.radians(angle))
        if (angle == 0) or (math.fabs(angle) == 180):
            deltaY = 0
        else:
            deltaY = scale_module * math.sin(math.radians(angle))
        circ_x.append(round_zero(x_list[j] + deltaX))
        circ_y.append(round_zero(y_list[j] + deltaY))
        j = i


# the fourth plot shows the data scaled with min_distance
ax4 = figure.add_subplot(gs[plot])
ax4.plot(circ_x, circ_y, 'ro', circ_x, circ_y, 'r-')
plot = plot + 1
seen = []
k = 1
for x, y in zip(circ_x, circ_y):
    if [x, y] not in seen:
        seen.append([x, y])
        ax4.annotate(
            'point{0}'.format(k),
            xy=(x, y), xytext=(-5, 5),
            textcoords='offset points', ha='right', va='bottom',
            bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.5),
        )
        k = k + 1

plt.show()
