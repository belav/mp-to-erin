import React, { useContext, useState } from "react";

export const AppContext = React.createContext({
    printWidth: 100,
    setPrintWidth: (value: number) => {},
});

export const useAppContext = () => useContext(AppContext);

// I regret trying out this approach to managing state....
export const useSetupAppContext = () => {
    const [printWidth, setPrintWidth] = useState(100);
    
    return {
        printWidth,
        setPrintWidth,
    };
};
