import matplotlib.pyplot as plt
import matplotlib.gridspec as gridspec
import numpy as np
from scipy.interpolate import interp1d
import re
from collections import OrderedDict

# open file with data and plot them
in_file = open("./measures.txt","r")
accX = []
moveX = []
accY = []
moveY = []
x = []
y = []

#figure = plt.figure()

entry = in_file.readline()
while len(entry) > 1:
    if entry.startswith("accX"):
        [a,b] = re.findall("[-+]?[.]?[\d]+(?:,\d\d\d)*[\.]?\d*(?:[eE][-+]?\d+)?", entry)
        accX.append(int(a))
        accY.append(int(b))
    elif entry.startswith("moveX"):
        [a,b] = re.findall("[-+]?[.]?[\d]+(?:,\d\d\d)*[\.]?\d*(?:[eE][-+]?\d+)?", entry)
        moveX.append(int(a))
        moveY.append(int(b))
    entry = in_file.readline()
in_file.close()

i = 0
while i < len(moveX) - 1:
    if (moveY[i] == 0) and not (moveX[i] == 0):
        j = i + 1
        while (j < len(moveY)) and not (moveY[j] == 1):
            if moveX[j] == -moveX[i]:
                moveX[j] = 0
            j = j + 1
        i = j
    else:
        i = i + 1

r_indexes = []
for i in range(len(moveX)):
    if moveX[i] == 0 and moveY[i] == 0:
        r_indexes.append(i)

for i in reversed(r_indexes):
    moveX.pop(i)
    moveY.pop(i)


x.append(0)
y.append(0)
lastx = 0
lasty = 0
for i in range(len(moveX)):
    posx = lastx + moveX[i]
    x.append(posx)
    posy = lasty + moveY[i]
    y.append(posy)
    lastx = posx
    lasty = posy


t1 = range(len(moveX))
t2 = range(len(x))

ax1 = plt.subplot2grid((4, 2), (0, 0))
ax1.plot(t1, moveX, 'ro', t1, moveX, 'r-')
ax1.set_title("Movement x")
ax2 = plt.subplot2grid((4, 2), (1, 0))
ax2.plot(t2, x, 'ro', t2, x, 'r-')
ax2.set_title("Position x")
ax3 = plt.subplot2grid((4, 2), (2, 0))
ax3.plot(t1, moveY, 'bo', t1, moveY, 'b-')
ax3.set_title("Movement y")
ax4 = plt.subplot2grid((4, 2), (3, 0))
ax4.plot(t2, y, 'bo', t2, y, 'b-')
ax4.set_title("Position x")
ax5 = plt.subplot2grid((4,2), (0, 1), rowspan=4)
ax5.plot(x, y, 'go', x, y, 'g-')
ax5.set_title("Movement 2D")
k = 0
#seen = []
for a, b in zip(x, y):
#    if [a, b] not in seen:
    k = k + 1
#        seen.append([a, b])
    ax5.annotate(
        'point{0}'.format(k),
        xy=(a, b), xytext=(-5, 5),
        textcoords='offset points', ha='right', va='bottom',
        bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.5),
    )

plt.show()
