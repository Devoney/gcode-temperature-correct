### The problem
Standbye temperatures can be used for idex (independant extruders) printers to reduce heat stress (or whatever) on an extruder and or its filament when idle.
However, when the warming up is not properly timed this causes delays during printing as the printer waits for the extruder to reach the correct temperature.
In some slicers (like with Cura + Snapmaker J1) it ignores the standbye temperature being set to equal the printing temperature. Resulting in standbye temperatures still.
Very annoying!!

### A solution
This console app replaces standbye temperatures in gcode for idex (independant extruders) for those slicers that ignore the standbye temperature having been set to equal the printing temperature.

#### Usage example
```
gcode-temperature-correct.exe --file "C:\my.gcode" --t0 210 --t1 235
```

This will then replace gcode such as 
```
T0
M104 T1 S220 ; This will be replaced
M109 S210
... ; Left out for brevity
T1
M104 T0 S195 ; This will be replaced
M109 S235
```

to 
```
T0
M104 T1 S235
M109 S210
... ; Left out for brevity
T1
M104 T0 S210
M109 S235
```
