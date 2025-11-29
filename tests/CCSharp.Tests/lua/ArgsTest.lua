function Start(arguments)
 print("Arguments:")
 for _,value in ipairs(arguments) do
  print(value)
 end
end
Start({...})