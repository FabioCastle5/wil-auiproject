import matplotlib.pyplot as plt
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
# vx_list = []
# vy_list = []
# ax_list = []
# ay_list = []

entry = in_file.readline()
samples = 0

while len(entry) > 1:
    if not entry.startswith("//") and not entry.startswith("---"):
        # if entry.startswith("---"):
        #     samples = samples + 1
        # else:
        [x,y] = re.findall("[-+]?[.]?[\d]+(?:,\d\d\d)*[\.]?\d*(?:[eE][-+]?\d+)?", entry)
        if entry.startswith("x"):
            x_list.append(np.floor(float(x)))
            y_list.append(np.floor(float(y)))
        # elif entry.startswith("velX"):
        #     vx_list.append(np.floor(float(x)))
        #     vy_list.append(np.floor(float(y)))
        # else:
        #     ax_list.append(int(x))
        #     ay_list.append(int(y))
    entry = in_file.readline()
in_file.close()

# remove the directions that won't be recognized by the videogame up to 45 degrees
r_indexes = []
j = 1
# first direction in between p0 and p1
a0 = math.degrees(math.pi + math.atan2(y_list[1]-y_list[0], x_list[1]-x_list[0]))
for i in xrange(2, len(x_list) - 1):
    # evaluate the angle respect to the previous point
    a = math.degrees(math.pi + math.atan2(y_list[i]-y_list[j], x_list[i]-x_list[j]))
    # if the angle is too high, that point will be deleted
    if abs(a - a0) > 45:
        r_indexes.append(i)
    else:
        a0 = a
        j = i

for i in reversed(r_indexes):
    x_list.pop(i)
    y_list.pop(i)
    vx_list.pop(i)
    vy_list.pop(i)
    ax_list.pop(i)
    ay_list.pop(i)

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
    vx_list.pop(i)
    vy_list.pop(i)
    ax_list.pop(i)
    ay_list.pop(i)

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

#
#
# x2 = []
# y2 = []
#
# for i in range(len(vx_list)):
#     if i > 0:
#         x = vx_list[i]
#         y = vy_list[i]
#         if not x == x0 and not y == y0:
#             x2.append(x)
#             y2.append(y)
#             x0 = x
#             y0 = y
#     else:
#         x0 = vx_list[i]
#         y0 = vy_list[i]
#
# vx_list = x2
# vy_list = y2

# # evaluate the mean values
# sum_x = 0
# sum_y = 0
# # sum_z = 0
# for i in x_list:
#     sum_x = sum_x + i
# for j in y_list:
#     sum_y = sum_y + j
# # for k in z_list:
# #     sum_z = sum_z + k
#
# mean_x = sum_x / samples
# mean_y = sum_y / samples
# # mean_z = sum_z / samples

# plot the results
# step = 2
# i = range(len(x_list[::step]))
# j = range(len(vx_list[::step]))
# fig, [vxPlot, xPlot, vyPlot, yPlot, posPlot] = plt.subplots(5,1)
# aPlot.set_title("Acceleration")
# aPlot.plot(ax_list, ay_list, 'ro')
# vPlot.set_title("Speed")
# vPlot.plot(vx_list, vy_list, 'bo')
# xPlot.set_title("Position")
# xPlot.plot(x_list, y_list, 'g')

# s = interp1d(x_list[::step], y_list[::step], kind='linear')
# vx = interp1d(j, vx_list[::step], kind='cubic')
# x = interp1d(i, x_list[::step], kind='cubic')
# vy = interp1d(j, vy_list[::step], kind='cubic')
# y = interp1d(i, y_list[::step], kind='cubic')
# plt.plot(x_list, y_list, 'ro', x_list, s(x_list), 'g-')

# vxPlot.set_title("Speed x")
# vxPlot.plot(j, vx(j), 'b-')
# vyPlot.set_title("Speed y")
# vyPlot.plot(j, vy(j), 'r-')
# xPlot.set_title("Position x")
# xPlot.plot(i, x(i), 'y-')
# yPlot.set_title("Position y")
# yPlot.plot(i, y(i), 'g-')
# posPlot.plot(x_list, s(x_list), 'ro', x_list, s(x_list), 'y-')
i = range(5)
plt.plot(x_list, y_list, 'ro', x_list, y_list, 'r-')
plt.axes().grid(color='g', linestyle='--', linewidth=2)

seen = []
k = 1
for x, y in zip(x_list, y_list):
    if [x, y] not in seen:
        seen.append([x, y])
        plt.annotate(
            'point{0}'.format(k),
            xy=(x, y), xytext=(-20, 20),
            textcoords='offset points', ha='right', va='bottom',
            bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.5),
        )
        k = k + 1

plt.show()
