local testVector = vector.new(5, 3, 2);
function Start()
 local testVector = testVector;
 print(("("..tostring((testVector~=nil) and testVector or nil))..")")
 print(testVector+vector.new(4, 3, 1))
 print(testVector*4)
 print(testVector/2)
 print(-testVector)
 print(testVector==vector.new(5, 3, 2))
 print(testVector==vector.new(4, 3, 1))
 print(vector.new())
end
Start()