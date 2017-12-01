import matplotlib.pyplot as plt
import matplotlib.gridspec as gridspec
import numpy as np
from scipy.interpolate import interp1d
import re

def round_zero(val):
    if val > 0:
        return np.floor(val)
    elif val < 0:
        return np.ceil(val)
    else:
        return val;

# open file with data and plot them
in_file = open("./measures.txt","r")
x_list = []
y_list = []

# prepare a figure for nPlot x 1 plots
nPlot = 2
plot = 0
figure = plt.figure()
gs = gridspec.GridSpec(nPlot, 1)


entry = in_file.readline()
while len(entry) > 1:
    if entry.startswith("accX"):
        [x,y] = re.findall("[-+]?[.]?[\d]+(?:,\d\d\d)*[\.]?\d*(?:[eE][-+]?\d+)?", entry)
        x_list.append(int(x))
        y_list.append(int(y))
    entry = in_file.readline()
in_file.close()

t = range(len(x_list))

ax1 = figure.add_subplot(gs[0])
ax1.plot(t, x_list, 'ro', t, x_list, 'r-')
ax2 = figure.add_subplot(gs[1])
ax2.plot(t, y_list, 'bo', t, y_list, 'b-')

plt.show()
